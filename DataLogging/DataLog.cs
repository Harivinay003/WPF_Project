using FluentModbus;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace DataLogging
{
    public class DataLog : IDisposable, IHostedService
    {
        // ---- Static config caches (populated once in StartAsync) ----

        public static List<AlarmTag> AlarmTags = new List<AlarmTag>();
        public static List<AlarmParameter> AlarmParameters = new List<AlarmParameter>();
        public static List<Category> Categories = new List<Category>();
        public static List<FieldDevice> FieldDevices = new List<FieldDevice>();
        public static List<IODevice> IODevices = new List<IODevice>();
        public static List<SerialDevice> SerialDevices = new List<SerialDevice>();
        public static List<SerialDeviceDriver> SerialDeviceDrivers = new List<SerialDeviceDriver>();
        public static List<SerialDeviceParameter> SerialDeviceParameters = new List<SerialDeviceParameter>();
        public static List<SerialDeviceReadBlock> SerialDeviceReadBlocks = new List<SerialDeviceReadBlock>();
        public static List<SerialDeviceRegister> SerialDeviceRegisters = new List<SerialDeviceRegister>();
        public static List<Tag> Tags = new List<Tag>();
        public static List<TrendTag> TrendTags = new List<TrendTag>();
        public static List<TrendParameter> TrendParameters = new List<TrendParameter>();

        // ---- O(1) lookup tables built once in StartAsync instead of LINQ scans on every tag, every cycle ----
        private Dictionary<(int devId, int tagId), AlarmTag> _alarmTagLookup = new();
        private Dictionary<(int serDevId, int paramId), AlarmParameter> _alarmParamLookup = new();
        private HashSet<(int devId, int tagId)> _trendTagLookup = new();
        private HashSet<(int serDevId, int paramId)> _trendParamLookup = new();

        System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();
        string SoundFile = AppDomain.CurrentDomain.BaseDirectory + "/Conf/BEEP.wav";
        bool buzzer = false;
        string DataConString, ConfigConString;

        private readonly List<Timer> Timers = new List<Timer>();

        // cache of databases already created in this process to avoid repeated CREATE calls
        private readonly ConcurrentDictionary<string, byte> _createdDatabases =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim _dbCreationLock = new SemaphoreSlim(1, 1);
        // One semaphore per device so a slow scan (Modbus timeout, DB latency) can't pile up overlapping
        // DoWork runs for the same device once the 5s timer fires again.
        private readonly ConcurrentDictionary<int, SemaphoreSlim> _deviceLocks = new();

        private Timer? _monthlyDbTimer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly AnalogBatcher _analogBatcher;

        // keep static lists as before, timers etc.

        public DataLog(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            ConfigConString = _configuration.GetConnectionString("ConfigDBConnString");
            DataConString = _configuration.GetConnectionString("DataDBConnString");
            soundPlayer.SoundLocation = SoundFile;
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.WriteLog("Exception: " + (e.ExceptionObject as Exception).Message);
        }

        // Ensure monthly database exists and basic schema required by InsertAnalogValue is present.
        private async Task EnsureMonthlyDatabaseExistsAsync(DateTime time)
        {
            var dbName = time.ToString("MMM-yyyy", CultureInfo.InvariantCulture);

            // fast path: if created already during process lifetime skip
            if (_createdDatabases.ContainsKey(dbName))
                return;
            // Guard the actual CREATE DATABASE / CREATE TABLE work so concurrent device scans crossing a
            // month boundary at the same moment don't all race to create it.
            await _dbCreationLock.WaitAsync();

            try
            {
                if (_createdDatabases.ContainsKey(dbName))
                    return;
                var masterConn = DataConString + $";Initial Catalog=master;";
                using (var con = new SqlConnection(masterConn))
                {
                    await con.OpenAsync();
                    // create database if not exists
                    string createDbSql = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}];";
                    using (var cmd = new SqlCommand(createDbSql, con))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // create required tables inside the new database if they do not exist
                    // NOTE: AnalogData now uses a composite primary key (DevId, SerDevId, ParamId, LogTime)
                    // and does NOT use an identity Id column.
                    string createTables = $@"
USE [{dbName}];

IF OBJECT_ID('dbo.AnalogData','U') IS NULL
BEGIN
    CREATE TABLE dbo.AnalogData(
        DevId INT NOT NULL,
        SerDevId INT NOT NULL,
        ParamId INT NOT NULL,
        LogTime DATETIME NOT NULL,
        Value REAL NULL,
        CONSTRAINT PK_AnalogData PRIMARY KEY (DevId, SerDevId, ParamId, LogTime)
    );
END

IF OBJECT_ID('dbo.DigitalData','U') IS NULL
BEGIN
    CREATE TABLE dbo.DigitalData(
        TagID INT NOT NULL,
        OnTime DATETIME NOT NULL,
        OffTime DATETIME NULL,
        CONSTRAINT PK_DigitalData PRIMARY KEY (TagID,OnTime)
    );
END

IF OBJECT_ID('dbo.AlarmsData','U') IS NULL
BEGIN
    CREATE TABLE dbo.AlarmsData(
        AlarmId INT NOT NULL,
        AlarmType INT NOT NULL,
        LogTime DATETIME NOT NULL,
        Comment NVARCHAR(255) NULL,
        Value REAL NULL,
        ResetTime DATETIME NULL,
        Acknowledged bit NULL,
        CONSTRAINT PK_AlarmsData PRIMARY KEY (AlarmId, AlarmType, LogTime)
    );
END
";
                    using (var cmd = new SqlCommand(createTables, con))
                    {
                        cmd.CommandTimeout = 60;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    _createdDatabases[dbName] = 1;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog($"EnsureMonthlyDatabaseExists('{dbName}') failed: {ex.Message}");
            }
            finally
            {
                _dbCreationLock.Release();
            }
        }

        // Schedule a timer to create next month's database at midnight on the 1st day
        private async Task ScheduleMonthlyDatabaseCreation()
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime firstOfNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                //DateTime nextRun = new DateTime(firstOfNextMonth.Year, firstOfNextMonth.Month, 1, 0, 0, 0);
                TimeSpan due = firstOfNextMonth - now;
                if (due < TimeSpan.Zero) due = TimeSpan.FromSeconds(10);

                // dispose existing timer if any
                _monthlyDbTimer?.Dispose();
                _monthlyDbTimer = new Timer(async _ =>
                {
                    try
                    {
                        await EnsureMonthlyDatabaseExistsAsync(DateTime.Now);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog("Monthly DB timer error: " + ex.Message);
                    }
                    finally
                    {
                        // reschedule for next month
                        await ScheduleMonthlyDatabaseCreation();
                    }
                }, null, due, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                Log.WriteLog("ScheduleMonthlyDatabaseCreation failed: " + ex.Message);
            }
        }

        private async Task InsertAlarmAsync(SqlConnection con, SqlTransaction tx,
            int AlarmId, int AlarmType, string comment, float Value, DateTime time)
        {

            const string insSql = @"IF NOT EXISTS(
            SELECT 1 FROM AlarmsData
            WHERE AlarmId = @AlarmId AND AlarmType = @AlarmType AND ResetTime IS NULL)
            BEGIN
            INSERT INTO dbo.AlarmsData (AlarmId, AlarmType, LogTime, Comment, Value, Acknowledged)
            VALUES (@AlarmId, @AlarmType, @LogTime, @Comment, @Value,0);
            END";

            using var insCommand = new SqlCommand(insSql, con, tx);
            insCommand.Parameters.Add("@AlarmId", SqlDbType.Int).Value = AlarmId;
            insCommand.Parameters.Add("@AlarmType", SqlDbType.Int).Value = AlarmType;
            insCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = time;
            insCommand.Parameters.Add("@Comment", SqlDbType.VarChar).Value = comment;
            insCommand.Parameters.Add("@Value", SqlDbType.Float).Value = Value;
            try { await insCommand.ExecuteNonQueryAsync(); }
            catch (Exception ex)
            { Log.WriteLog("InsertAlarm: AlarmId- " + AlarmId + " Type- " + AlarmType + ", Message: " + ex.Message); }
        }

        private async Task UpdateAlarmAsync(SqlConnection con, SqlTransaction tx,
            int AlarmId, int AlarmType, DateTime time)
        {
            string updateSql = @"UPDATE AlarmsData SET ResetTime = @ResetTime
                                        WHERE AlarmId = @AlarmId AND AlarmType = @AlarmType AND ResetTime IS NULL";

            using var cmd = new SqlCommand(updateSql, con, tx);
            cmd.Parameters.Add("@AlarmId", SqlDbType.Int).Value = AlarmId;
            cmd.Parameters.Add("@AlarmType", SqlDbType.Int).Value = AlarmType;
            cmd.Parameters.Add("@ResetTime", SqlDbType.DateTime).Value = time;
            try { await cmd.ExecuteNonQueryAsync(); }
            catch (Exception ex)
            { Log.WriteLog("UpdateAlarm: AlarmId: " + AlarmId + " Type - " + AlarmType + " - " + ex.Message); }
        }

        private async Task InsertAnalogValueAsync(SqlConnection con, SqlTransaction tx,
            int devId, int serDevId, int ParamId, double Value, DateTime time)
        {
            DateTime hTime = time.AddSeconds(-time.Second).AddMilliseconds(-time.Millisecond);
            // Keep immediate insert for analog values to preserve uniqueness behavior

            con.Open();
            string insSql = @"
            IF NOT EXISTS(SELECT 1 FROM AnalogData 
            WHERE DevId = @DevId AND SerDevId = @SerDevId AND ParamId = @ParamId AND LogTime = @LogTime)
            BEGIN
            INSERT INTO AnalogData (DevId, SerDevId, ParamId, LogTime, Value)
            VALUES (@DevId, @SerDevId, @ParamId, @LogTime, @Value);
            END";

            using var insCommand = new SqlCommand(insSql, con);
            insCommand.Parameters.Add("@DevId", SqlDbType.Int).Value = devId;
            insCommand.Parameters.Add("@SerDevId", SqlDbType.Int).Value = serDevId;
            insCommand.Parameters.Add("@ParamId", SqlDbType.Int).Value = ParamId;
            insCommand.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = hTime;
            insCommand.Parameters.Add("@Value", SqlDbType.Float).Value = Value;
            try
            {
                await insCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Log.WriteLog("InsertAnalogValue error: ParamId: " + ParamId + " Value - " + Value + " - " + ex.Message);
                throw;
            }
        }

        private static async Task InsertTrendValueAsync(SqlConnection con, SqlTransaction tx,
           int devId, int serDevId, int paramId, double value, DateTime time)
        {
            DateTime hTime = time.AddMilliseconds(-time.Millisecond);
            const string sql = @"INSERT INTO dbo.AnalogData (DevId, SerDevId, ParamId, LogTime, Value)
                                  VALUES (@DevId, @SerDevId, @ParamId, @LogTime, @Value);";
            using (var cmd = new SqlCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@DevId", SqlDbType.Int).Value = devId;
                cmd.Parameters.Add("@SerDevId", SqlDbType.Int).Value = serDevId;
                cmd.Parameters.Add("@ParamId", SqlDbType.Int).Value = paramId;
                cmd.Parameters.Add("@LogTime", SqlDbType.DateTime).Value = hTime;
                cmd.Parameters.Add("@Value", SqlDbType.Float).Value = value;
                try { await cmd.ExecuteNonQueryAsync(); }
                catch (Exception ex)
                { Log.WriteLog("InsertTrendValue: ParamId " + paramId + " - " + ex.Message); }
            }
        }

        private static async Task UpsertLiveAsync(SqlConnection con, SqlTransaction tx,
            int devId, int serDevId, int paramId, double value, DateTime time)
        {
            const string sql = @"
            MERGE dbo.Live WITH (HOLDLOCK) AS target
            USING (SELECT @DevId AS DevId, @SerDevId AS SerDevId, @ParamId AS ParamId) AS src
                ON target.DevId = src.DevId AND target.SerDevId = src.SerDevId AND target.ParamId = src.ParamId
            WHEN MATCHED THEN
                UPDATE SET Time = @Time, Value = @Value
            WHEN NOT MATCHED THEN
                INSERT (DevId, SerDevId, ParamId, Time, Value) VALUES (@DevId, @SerDevId, @ParamId, @Time, @Value);";

            using var cmd = new SqlCommand(sql, con, tx);
            cmd.Parameters.Add("@DevId", SqlDbType.Int).Value = devId;
            cmd.Parameters.Add("@SerDevId", SqlDbType.Int).Value = serDevId;
            cmd.Parameters.Add("@ParamId", SqlDbType.Int).Value = paramId;
            cmd.Parameters.Add("@Time", SqlDbType.DateTime).Value = time;
            cmd.Parameters.Add("@Value", SqlDbType.Float).Value = value;
            try { await cmd.ExecuteNonQueryAsync(); }
            catch (Exception ex) { Log.WriteLog("UpsertLive: " + ex.Message); }
        }

        private async Task EvaluateAlarmAsync(SqlConnection con, SqlTransaction tx,
            int alarmId, int alarmType, double value, float? low, float? high, DateTime time)
        {
            if (low.HasValue && value < low.Value)
                await InsertAlarmAsync(con, tx, alarmId, alarmType, "Low Set Point Alarm", (float)value, time);
            else if (high.HasValue && value > high.Value)
                await InsertAlarmAsync(con, tx, alarmId, alarmType, "High Set Point Alarm", (float)value, time);
            else
                await UpdateAlarmAsync(con, tx, alarmId, alarmType, time);
        }

        // buf/offset addressing is in bytes, offset = (tagAddress - blockStartAddress) * 2
        private static double DecodeValue(byte[] buf, int offset, DataType type, bool swapRegs = false)
        {
            switch (type)
            {
                case DataType.INT:
                    return BitConverter.ToInt16(new[] { buf[offset + 1], buf[offset] }, 0);
                case DataType.UINT:
                case DataType.WBOOL: // word value; bit extraction happens by the caller for WBOOL
                    return BitConverter.ToUInt16(new[] { buf[offset + 1], buf[offset] }, 0);
                case DataType.DINT:
                    return BitConverter.ToInt32(new[] { buf[offset + 3], buf[offset + 2], buf[offset + 1], buf[offset] }, 0);
                case DataType.UDINT:
                    return BitConverter.ToUInt32(new[] { buf[offset + 3], buf[offset + 2], buf[offset + 1], buf[offset] }, 0);
                case DataType.REAL:
                    return swapRegs
                        ? BitConverter.ToSingle(new[] { buf[offset + 3], buf[offset + 2], buf[offset + 1], buf[offset] }, 0)
                        : BitConverter.ToSingle(new[] { buf[offset + 1], buf[offset], buf[offset + 3], buf[offset + 2] }, 0);
                case DataType.INT64:
                    {
                        // NOTE: the original PLC/EthernetDevice branch had a copy-paste bug here
                        // (`longbytes[6]` used twice, so `real2` never read the low half correctly).
                        // Fixed to mirror the Gateway branch's correct byte order below.
                        float hi = BitConverter.ToSingle(new[] { buf[offset + 7], buf[offset + 6], buf[offset + 5], buf[offset + 4] }, 0);
                        float lo = BitConverter.ToSingle(new[] { buf[offset + 1], buf[offset], buf[offset + 3], buf[offset + 2] }, 0);
                        return hi * 4294967.296 + (lo / 1000.0);
                    }
                default:
                    throw new NotSupportedException($"DecodeValue: unsupported {type}");
            }
        }

        private async Task ProcessValueAsync(SqlConnection dataCon, SqlTransaction dataTx,
            SqlConnection configCon, SqlTransaction configTx,
            int devId, int serDevId, int paramId, double value, DateTime time,
            bool isTrend, int alarmId, int alarmType, float? low, float? high, bool hasAlarm)
        {
            await UpsertLiveAsync(configCon, configTx, devId, serDevId, paramId, value, time);
            if (isTrend)
                await InsertTrendValueAsync(dataCon, dataTx, devId, serDevId, paramId, value, time);
            await InsertAnalogValueAsync(dataCon, dataTx, devId, serDevId, paramId, value, time);
            if (hasAlarm)
                await EvaluateAlarmAsync(dataCon, dataTx, alarmId, alarmType, value, low, high, time);
        }

        private static async Task UpdateDigitalDataAsync(SqlConnection con, SqlTransaction tx, int tagId, int value, DateTime time)
        {
            const string sql = @"
            DECLARE @Open INT = (SELECT TOP 1 TagID FROM dbo.DigitalData WHERE TagID=@TagId AND OffTime IS NULL);
            IF @Open IS NULL AND @Value = 1
            BEGIN
                INSERT INTO dbo.DigitalData (TagID, OnTime) VALUES (@TagId, @Time);
            END
            ELSE IF @Open IS NOT NULL AND @Value = 0
            BEGIN
                UPDATE dbo.DigitalData SET OffTime = @Time WHERE TagID=@TagId AND OffTime IS NULL;
            END";
            using var cmd = new SqlCommand(sql, con, tx);
            cmd.Parameters.Add("@TagId", SqlDbType.Int).Value = tagId;
            cmd.Parameters.Add("@Value", SqlDbType.Int).Value = value;
            cmd.Parameters.Add("@Time", SqlDbType.DateTime).Value = time;
            try { await cmd.ExecuteNonQueryAsync(); }
            catch (Exception ex) { Log.WriteLog("digital Update: TagId " + tagId + " Value " + value + " - " + ex.Message); }
        }

        private static int GetRegisterCount(DataType type) => type switch
        {
            DataType.INT or DataType.UINT or DataType.WBOOL => 1,
            DataType.DINT or DataType.UDINT or DataType.REAL => 2,
            DataType.INT64 => 4,
            _ => throw new NotSupportedException($"GetRegisterCount: unsupported {type}")
        };

        // Groups tags/registers by proximity so we issue ONE ReadHoldingRegisters call per cluster
        // instead of one call per tag. Mirrors the SerialDeviceReadBlock concept that already exists
        // for the Gateway path, applied at runtime for PLC/EthernetDevice tags.
        private class RegisterBlock
        {
            public int StartAddress;
            public int RegisterCount;
            public readonly List<Tag> Tags = new();
        }

        private static List<RegisterBlock> BuildRegisterBlocks(IEnumerable<Tag> tags, int maxGapRegisters = 10, int maxBlockRegisters = 120)
        {
            var blocks = new List<RegisterBlock>();
            var sorted = tags.Where(t => t.Type != DataType.BOOL).OrderBy(t => t.Address).ToList();

            RegisterBlock current = null;
            foreach (var tag in sorted)
            {
                int regCount = GetRegisterCount(tag.Type);
                int tagEndExclusive = tag.Address + regCount;

                if (current == null
                    || tag.Address - (current.StartAddress + current.RegisterCount) > maxGapRegisters
                    || tagEndExclusive - current.StartAddress > maxBlockRegisters)
                {
                    current = new RegisterBlock { StartAddress = tag.Address, RegisterCount = regCount };
                    blocks.Add(current);
                }
                else
                {
                    current.RegisterCount = Math.Max(current.RegisterCount, tagEndExclusive - current.StartAddress);
                }
                current.Tags.Add(tag);
            }
            return blocks;
        }

        // Same idea for coil-based BOOL tags, using ReadCoils once per cluster.
        private static List<(int StartAddress, int Count, List<Tag> Tags)> BuildCoilBlocks(IEnumerable<Tag> tags, int maxGapCoils = 16, int maxBlockCoils = 500)
        {
            var blocks = new List<(int, int, List<Tag>)>();
            var sorted = tags.Where(t => t.Type == DataType.BOOL).OrderBy(t => t.Address).ToList();

            int start = -1, count = 0;
            List<Tag> current = null;
            foreach (var tag in sorted)
            {
                if (current == null || tag.Address - (start + count) > maxGapCoils || (tag.Address - start + 1) > maxBlockCoils)
                {
                    start = tag.Address;
                    count = 1;
                    current = new List<Tag>();
                    blocks.Add((start, count, current));
                }
                else
                {
                    count = tag.Address - start + 1;
                    // update the tuple's Count via replacing last entry (tuples are value types)
                    blocks[^1] = (start, count, current);
                }
                current.Add(tag);
            }
            return blocks;
        }


        // =========================================================================================
        // Device polling
        // =========================================================================================
        private async Task DoWorkAsync(IODevice device)
        {
            var gate = _deviceLocks.GetOrAdd(device.Id, _ => new SemaphoreSlim(1, 1));
            if (!await gate.WaitAsync(0))
            {
                // Another scan for this device is already in progress, skip this cycle
                return;
            }
            try
            {
                DateTime time = DateTime.Now;
                await EnsureMonthlyDatabaseExistsAsync(time);

                string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)};";
                using var dataCon = new SqlConnection(dconString);
                await dataCon.OpenAsync();
                using var dataTx = dataCon.BeginTransaction();

                using var configCon = new SqlConnection(ConfigConString);
                await configCon.OpenAsync();
                using var configTx = configCon.BeginTransaction();

                switch (device.DeviceType)
                {
                    case IODeviceType.Gateway:
                        await PollGatewayAsync(device, dataCon, dataTx, configCon, configTx, time);
                        break;
                    case IODeviceType.PLC:
                    case IODeviceType.EthernetDevice:
                        await PollEthernetDeviceAsync(device, dataCon, dataTx, configCon, configTx, time);
                        break;
                }
                dataTx.Commit();
                configTx.Commit();
            }
            catch (Exception ex)
            {
                Log.WriteLog("DoWorkAsync error for " + device.Name + ": " + ex.Message);
            }
            finally
            {
                gate.Release();
            }
        }

        private async Task PollEthernetDeviceAsync(IODevice device, SqlConnection dataCon, SqlTransaction dataTx,
            SqlConnection configCon, SqlTransaction configTx, DateTime time)
        {
            var tags = Tags.Where(t => t.DeviceId == device.Id).ToList();
            using var modbusClient = new ModbusTcpClient();
            try
            {
                modbusClient.Connect(new IPEndPoint(IPAddress.Parse(device.IpAddress), 502));
            }
            catch (Exception ex)
            {
                Log.WriteLog("Can't connect to " + device.Name + ": " + ex.Message);
                return;
            }
            if (!modbusClient.IsConnected) return;

            const int unitIdentifier = 1; // Assuming unit identifier is 1 for Ethernet devices

            // --- Holding register tags: batched, async ReadHoldingRegisters instead of one call per tag ---
            foreach (var block in BuildRegisterBlocks(tags))
            {
                byte[] readBytes;
                try
                {
                    readBytes = (await modbusClient.ReadHoldingRegistersAsync<byte>(unitIdentifier, block.StartAddress, block.RegisterCount * 2)).ToArray();
                }
                catch (Exception ex)
                {
                    Log.WriteLog($"Read from device {device.Name}, block {block.StartAddress}, count {block.RegisterCount}: {ex.Message}");
                    continue;
                }
                foreach (var tag in block.Tags)
                {
                    int buffer = (tag.Address - block.StartAddress) * 2; // Assuming each register is 2 bytes
                    try
                    {
                        if (tag.Type == DataType.WBOOL)
                        {
                            var bitArray = new BitArray(new byte[] { readBytes[buffer + 1], readBytes[buffer] });
                            int wres = Convert.ToInt32(bitArray[tag.Bit]);
                            await UpsertLiveAsync(configCon, configTx, device.Id, 0, tag.Id, wres, time);
                            await UpdateDigitalDataAsync(dataCon, dataTx, tag.Id, wres, time);
                            continue;
                        }
                        double value = DecodeValue(readBytes, buffer, tag.Type);
                        bool isTrend = _trendTagLookup.Contains((device.Id, tag.Id));
                        _alarmTagLookup.TryGetValue((device.Id, tag.Id), out var alarmTag);
                        await ProcessValueAsync(dataCon, dataTx, configCon, configTx,
                            device.Id, 0, tag.Id, value, time,
                            isTrend,
                            alarmTag?.Id ?? 0, 1,
                            alarmTag?.LowSetPoint, alarmTag?.HighSetPoint,
                            alarmTag != null);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog($"Process error tag {tag.Id} on {device.Name}: {ex.Message}");
                    }
                }
            }

            // --- Coil (BOOL) tags: batched, async ReadCoils instead of one call per tag ---
            foreach (var (start, count, coilTags) in BuildCoilBlocks(tags))
            {
                byte[] coilBytes;
                try
                {
                    coilBytes = (await modbusClient.ReadCoilsAsync(unitIdentifier, start, count)).ToArray();
                }
                catch (Exception ex)
                {
                    Log.WriteLog($"Read coils from device {device.Name}, start {start}, count {count}: {ex.Message}");
                    continue;
                }
                foreach (var tag in coilTags)
                {
                    try
                    {
                        int bitIndex = tag.Address - start;
                        int byteIndex = bitIndex / 8;
                        int bitInByte = bitIndex % 8;
                        bool boolRes = (coilBytes[byteIndex] & (1 << bitInByte)) != 0;
                        int res = Convert.ToInt32(boolRes);
                        await UpsertLiveAsync(configCon, configTx, device.Id, 0, tag.Id, res, time);
                        await UpdateDigitalDataAsync(dataCon, dataTx, tag.Id, res, time);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog($"Process coil tag {tag.Id} on {device.Name}: {ex.Message}");
                    }
                }
            }
            modbusClient.Disconnect();
        }

        private async Task PollGatewayAsync(IODevice device, SqlConnection dataCon, SqlTransaction dataTx,
            SqlConnection configCon, SqlTransaction configTx, DateTime time)
        {
            var serialDevices = SerialDevices.Where(sd => sd.Gateway == device).ToList();
            var modbusClient = new ModbusTcpClient();
            try
            {
                modbusClient.Connect(new IPEndPoint(IPAddress.Parse(device.IpAddress), 502));
            }
            catch (Exception ex)
            {
                Log.WriteLog("Can't connect to " + device.Name + ": " + ex.Message);
                return;
            }

            if (!modbusClient.IsConnected) return;

            foreach (var serialDevice in serialDevices)
            {
                int unitIdentifier = serialDevice.UnitId;
                foreach (var block in serialDevice.Driver.ReadBlocks)
                {
                    int startAddress = block.StartAddress;
                    int count = block.Count * 2; // Assuming each register is 2 bytes
                    int endAddress = startAddress + count - 1;
                    byte[] readBytes;
                    try
                    {
                        readBytes = (await modbusClient.ReadHoldingRegistersAsync<byte>(unitIdentifier, startAddress, count)).ToArray();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog($"Read from serial device {serialDevice.Name}, block {startAddress}, count {count}: {ex.Message}");
                        continue;
                    }

                    var registers = serialDevice.Driver.ReadRegisters.Where(r => r.RegisterAddress >= startAddress && r.RegisterAddress <= endAddress);
                    foreach (var reg in registers)
                    {
                        if (reg.DataType != DataType.REAL && reg.DataType != DataType.INT64)
                        {
                            Log.WriteLog($"Unsupported data type {reg.DataType} for register {reg.RegisterAddress} in device {serialDevice.Name}");
                            continue;
                        }

                        int paramId = reg.Parameter.Id;
                        int buffer = (reg.RegisterAddress - startAddress) * 2; // Assuming each register is 2 bytes
                        double value;

                        try
                        {
                            value = DecodeValue(readBytes, buffer, reg.DataType, serialDevice.SwapRegs);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog($"Decode error param {paramId}: {ex.Message}");
                            continue;
                        }

                        bool isTrend = _trendParamLookup.Contains((serialDevice.Id, paramId));
                        _alarmParamLookup.TryGetValue((serialDevice.Id, paramId), out var alarmDef);

                        await ProcessValueAsync(dataCon, dataTx, configCon, configTx,
                            device.Id, serialDevice.Id, paramId, value, time,
                            isTrend,
                            alarmDef?.Id ?? 0, 0,
                            alarmDef?.LowSetPoint, alarmDef?.HighSetPoint,
                            alarmDef != null);
                    }
                }
            }
            modbusClient.Disconnect();
        }

        public void Dispose()
        {
            foreach (var timer in Timers)
                timer?.Dispose();
            _monthlyDbTimer?.Dispose();
            foreach (var sem in _deviceLocks.Values)
                sem.Dispose();
            _dbCreationLock.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // resolve scoped services inside a scope
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<iDbRepository>();
                AlarmTags = repo.GetAlarmTags().ToList();
                AlarmParameters = repo.GetAlarmParameters().ToList();
                Categories = repo.GetCategories().ToList();
                FieldDevices = repo.GetFieldDevices().ToList();
                IODevices = repo.GetIODevices().ToList();
                SerialDevices = repo.GetSerialDevices().ToList();
                SerialDeviceDrivers = repo.GetSerialDeviceDrivers().ToList();
                SerialDeviceParameters = repo.GetSerialDeviceParameters().ToList();
                SerialDeviceReadBlocks = repo.GetSerialDeviceReadBlocks().ToList();
                SerialDeviceRegisters = repo.GetSerialDeviceRegisters().ToList();
                TrendParameters = repo.GetTrendParameters().ToList();
                TrendTags = repo.GetTrendTags().ToList();
                Tags = repo.GetTags().ToList();
            }
            // Build O(1) lookups once instead of scanning these lists on every tag on every 5s cycle.
            _alarmTagLookup = AlarmTags
                .Where(a => a.FieldDevice?.IODevice != null)
                .ToDictionary(a => (a.FieldDevice.IODevice.Id, a.TagId), a => a);
            _alarmParamLookup = AlarmParameters
                .ToDictionary(a => (a.SerialDeviceId, a.SerialDeviceParameterId), a => a);
            _trendTagLookup = TrendTags
                .Where(t => t.FieldDevice != null)
                .Select(t => (t.FieldDevice.IODeviceId, t.TagId))
                .ToHashSet();
            _trendParamLookup = TrendParameters
                .Select(t => (t.SerialDeviceId, t.SerialDeviceParameterId))
                .ToHashSet();

            // ensure current month DB exists immediately and schedule subsequent monthly creation
            await EnsureMonthlyDatabaseExistsAsync(DateTime.Now);
            await ScheduleMonthlyDatabaseCreation();
            foreach (var device in IODevices.Where(d => d != null))
            {
                try
                {
                    int second = 60 - DateTime.Now.Second;
                    Timers.Add(new Timer(OnDeviceTimer, device, second * 100, 500));
                    Log.WriteLog("DataLogging has started for " + device.Name);
                }
                catch (Exception ex)
                {
                    Log.WriteLog("OnStart-Exception: " + ex.Message);
                }
            }
            Timers.Add(new Timer(OnCheckAlarmsTimer, null, 0, 5000));
        }


        // Timer callback -> fire the async scan without blocking the timer thread, and without letting
        // an unobserved exception crash the process.
        private void OnDeviceTimer(object? state)
        {
            var device = (IODevice)state;
            _ = DoWorkAsync(device).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Log.WriteLog("Unhandled DoWorkAsync fault for " + device.Name + ": " + t.Exception?.Flatten().Message);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        private void OnCheckAlarmsTimer(object? state)
        {
            _ = CheckAlarmsAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Log.WriteLog("Unhandled CheckAlarmsAsync fault: " + t.Exception?.Flatten().Message);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (Timer timer in Timers)
            {
                if (timer != null)
                    timer.Dispose();
            }
            _monthlyDbTimer?.Dispose();
            return Task.CompletedTask;
        }

        public async Task CheckAlarmsAsync()
        {
            try
            {
                DateTime time = DateTime.Now;
                string dconString = DataConString + $";Initial Catalog={time.ToString("MMM-yyyy", CultureInfo.InvariantCulture)}";

                using SqlConnection connection = new SqlConnection(dconString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(
                   "SELECT TOP 1 AlarmId FROM dbo.AlarmsData WHERE ResetTime IS NULL AND Acknowledged = 0", connection);
                var resObj = await cmd.ExecuteScalarAsync();
                if (resObj != null)
                {
                    if (!buzzer)
                    {
                        soundPlayer.PlayLooping();
                        buzzer = true;
                    }
                }
                else
                {
                    soundPlayer.Stop();
                    buzzer = false;
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Log.WriteLog("CheckAlarms: " + ex.Message);
            }
        }
    }
}
