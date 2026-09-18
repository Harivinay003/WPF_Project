using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DataLogging
{
    internal class AnalogRow
    {
        public int DevId { get; set; }
        public int SerDevId { get; set; }
        public int ParamId { get; set; }
        public DateTime LogTime { get; set; }
        public double Value { get; set; }
    }

    internal class AnalogBatcher : IDisposable
    {
        private readonly string _dataConnBase;
        private readonly ConcurrentQueue<AnalogRow> _queue = new ConcurrentQueue<AnalogRow>();
        private readonly Timer _timer;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);
        private bool _disposed;

        public AnalogBatcher(string dataConnBase)
        {
            _dataConnBase = dataConnBase;
            _timer = new Timer(FlushTimer, null, _interval, _interval);
        }

        public void Enqueue(AnalogRow row)
        {
            _queue.Enqueue(row);
        }

        private void FlushTimer(object? state)
        {
            try
            {
                FlushAll();
            }
            catch (Exception ex)
            {
                Log.WriteLog("AnalogBatcher flush error: " + ex.Message);
            }
        }

        private void FlushAll()
        {
            if (_queue.IsEmpty) return;

            // Drain queue into list
            var rows = new List<AnalogRow>();
            while (_queue.TryDequeue(out var r)) rows.Add(r);
            if (rows.Count == 0) return;

            // Group by month DB
            var groups = rows.GroupBy(r => r.LogTime.ToString("MMM-yyyy", CultureInfo.InvariantCulture));
            foreach (var g in groups)
            {
                string dbName = g.Key;
                string conn = _dataConnBase + ";Initial Catalog=" + dbName;

                try
                {
                    // ensure DB and tables exist
                    EnsureMonthlyDatabaseExists(dbName);

                        using (var con = new SqlConnection(conn))
                        {
                            con.Open();
                            using (var tran = con.BeginTransaction())
                            {
                                try
                                {
                                    // Use parameterized IF NOT EXISTS -> INSERT for each row to avoid PK conflicts
                                    string insSql = @"
                                        IF NOT EXISTS(
                                            SELECT 1 FROM AnalogData
                                            WHERE DevId = @DevId AND SerDevId = @SerDevId AND ParamId = @ParamId AND LogTime = @LogTime
                                        )
                                        BEGIN
                                            INSERT INTO AnalogData (DevId, SerDevId, ParamId, LogTime, Value)
                                            VALUES (@DevId, @SerDevId, @ParamId, @LogTime, @Value);
                                        END
                                    ";

                                    using (var cmd = new SqlCommand(insSql, con, tran))
                                    {
                                        cmd.Parameters.Add(new SqlParameter("@DevId", SqlDbType.Int));
                                        cmd.Parameters.Add(new SqlParameter("@SerDevId", SqlDbType.Int));
                                        cmd.Parameters.Add(new SqlParameter("@ParamId", SqlDbType.Int));
                                        cmd.Parameters.Add(new SqlParameter("@LogTime", SqlDbType.DateTime));
                                        cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.Float));

                                        foreach (var row in g)
                                        {
                                            cmd.Parameters["@DevId"].Value = row.DevId;
                                            cmd.Parameters["@SerDevId"].Value = row.SerDevId;
                                            cmd.Parameters["@ParamId"].Value = row.ParamId;
                                            cmd.Parameters["@LogTime"].Value = row.LogTime;
                                            cmd.Parameters["@Value"].Value = row.Value;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }

                                    tran.Commit();
                                }
                                catch (Exception ex)
                                {
                                    try { tran.Rollback(); } catch { }
                                    Log.WriteLog("AnalogBatcher group insert failed for " + dbName + ": " + ex.Message);
                                }
                            }
                            con.Close();
                        }
                }
                catch (Exception ex)
                {
                    Log.WriteLog("AnalogBatcher group insert failed for " + dbName + ": " + ex.Message);
                }
            }
        }

        // Minimal DB/table creation similar to DataLog.EnsureMonthlyDatabaseExists
        private void EnsureMonthlyDatabaseExists(string dbName)
        {
            try
            {
                var masterConn = _dataConnBase + ";Initial Catalog=master;";
                using (var con = new SqlConnection(masterConn))
                {
                    con.Open();
                    string createDbSql = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}];";
                    using (var cmd = new SqlCommand(createDbSql, con)) cmd.ExecuteNonQuery();

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
";
                    using (var cmd = new SqlCommand(createTables, con)) cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("EnsureMonthlyDatabaseExists (batcher) failed: " + ex.Message);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timer.Dispose();
            // final flush
            try { FlushAll(); } catch { }
        }
    }
}
