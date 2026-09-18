using FluentModbus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace WPFSCADA.Services
{
    public class PLCService
    {
        private readonly iDbRepository _repository;

        public PLCService(iDbRepository repository)
        {
            _repository = repository;
        }

        public async Task<Dictionary<int, double>> ReadTagsAsync(
            IEnumerable<int> tagIds)
        {
            var tagUpdateActions = new Dictionary<int, double>();

            try
            {
                // Get PLC device
                var devices = _repository?
                    .GetIODevices()
                    .ToList();

                var plcDevice = devices?
                    .FirstOrDefault(d =>
                        d.DeviceType == IODeviceType.PLC ||
                        d.DeviceType == IODeviceType.EthernetDevice);

                if (plcDevice == null ||
                    string.IsNullOrWhiteSpace(plcDevice.IpAddress))
                {
                    Debug.WriteLine(
                        "PLC device not found or IP address not configured.");

                    return tagUpdateActions;
                }

                await ReadFromPLCAsync(
                    plcDevice,
                    tagIds,
                    tagUpdateActions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error reading values from PLC: {ex}");
            }

            return tagUpdateActions;
        }

        private async Task ReadFromPLCAsync(
            IODevice plcDevice,
            IEnumerable<int> tagIds,
            Dictionary<int, double> tagUpdateActions)
        {
            using var modbusClient = new ModbusTcpClient();

            try
            {
                Debug.WriteLine(
                    $"PLC: {plcDevice.Name}, " +
                    $"IP: {plcDevice.IpAddress}, " +
                    $"Port: 502");

                // Connect to PLC
                modbusClient.Connect(
                    new IPEndPoint(
                        IPAddress.Parse(plcDevice.IpAddress),
                        502));

                if (!modbusClient.IsConnected)
                {
                    Debug.WriteLine(
                        $"Failed to connect to PLC at " +
                        $"{plcDevice.IpAddress}");

                    return;
                }

                const int unitIdentifier = 1;

                // Get all tags
                var allTags = _repository?
                    .GetTags()
                    .ToList()
                    ?? new List<Tag>();

                var requestedTagIds = tagIds?
                    .Distinct()
                    .ToHashSet()
                    ?? new HashSet<int>();

                // Only read tags assigned to this PLC
                var tags = allTags
                    .Where(t =>
                        t.DeviceId == plcDevice.Id &&
                        requestedTagIds.Contains(t.Id))
                    .ToList();

                if (tags.Count == 0)
                {
                    Debug.WriteLine(
                        "No PLC tags found for the requested tag IDs.");

                    return;
                }

                // -----------------------------------------
                // HOLDING REGISTERS
                // -----------------------------------------

                foreach (var block in BuildRegisterBlocks(tags))
                {
                    byte[] readBytes;

                    try
                    {


                        readBytes =
                            (await modbusClient
                                .ReadHoldingRegistersAsync<byte>(
                                    unitIdentifier,
                                    block.StartAddress,
                                    block.RegisterCount * 2))
                            .ToArray();


                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            $"PLC REGISTER READ ERROR: {ex}");

                        continue;
                    }

                    foreach (var tag in block.Tags)
                    {
                        int buffer =
                            (tag.Address - block.StartAddress) * 2;

                        try
                        {
                            // WBOOL
                            if (tag.Type == DataType.WBOOL)
                            {
                                var bitArray =
                                    new BitArray(
                                        new byte[]
                                        {
                                            readBytes[buffer + 1],
                                            readBytes[buffer]
                                        });

                                int result =
                                    Convert.ToInt32(
                                        bitArray[tag.Bit]);

                                tagUpdateActions[tag.Id] = result;

                                continue;
                            }


                            double value =
                                DecodeValue(
                                    readBytes,
                                    buffer,
                                    tag.Type);



                            tagUpdateActions[tag.Id] = value;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(
                                $"Error processing tag " +
                                $"{tag.Id} ({tag.Name}): {ex.Message}");
                        }
                    }
                }

                // -----------------------------------------
                // COILS
                // -----------------------------------------

                foreach (var (start, count, coilTags)
                         in BuildCoilBlocks(tags))
                {
                    byte[] coilBytes;

                    try
                    {
                        coilBytes =
                            (await modbusClient
                                .ReadCoilsAsync(
                                    unitIdentifier,
                                    start,
                                    count))
                            .ToArray();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            $"Error reading coils from " +
                            $"{plcDevice.Name}: {ex.Message}");

                        continue;
                    }

                    foreach (var tag in coilTags)
                    {
                        try
                        {
                            int bitIndex =
                                tag.Address - start;

                            int byteIndex =
                                bitIndex / 8;

                            int bitInByte =
                                bitIndex % 8;

                            bool boolResult =
                                (coilBytes[byteIndex] &
                                 (1 << bitInByte)) != 0;

                            int result =
                                Convert.ToInt32(boolResult);

                            tagUpdateActions[tag.Id] = result;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(
                                $"Error processing coil tag " +
                                $"{tag.Id} ({tag.Name}): " +
                                $"{ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Exception in ReadFromPLCAsync: {ex}");
            }
            finally
            {
                if (modbusClient.IsConnected)
                {
                    modbusClient.Disconnect();
                }
            }
        }

        public async Task<bool> WriteBoolAsync(int tagId, bool value)
        {
            try
            {
                // Get tag
                var tag = _repository?
                    .GetTags()
                    .FirstOrDefault(t => t.Id == tagId);

                if (tag == null)
                {
                    Debug.WriteLine($"PLC tag not found. TagId={tagId}");
                    return false;
                }

                // Get the device assigned to this tag
                var plcDevice = _repository?
                    .GetIODevices()
                    .FirstOrDefault(d => d.Id == tag.DeviceId);

                if (plcDevice == null ||
                    string.IsNullOrWhiteSpace(plcDevice.IpAddress))
                {
                    Debug.WriteLine(
                        $"PLC device not found for TagId={tagId}");

                    return false;
                }

                // Make sure this is a supported BOOL type
                if (tag.Type != DataType.BOOL &&
                    tag.Type != DataType.WBOOL)
                {
                    Debug.WriteLine(
                        $"Tag {tag.Name} is {tag.Type}, not BOOL/WBOOL.");

                    return false;
                }

                using var modbusClient = new ModbusTcpClient();

                await Task.Run(() =>
                {
                    modbusClient.Connect(
                        new IPEndPoint(
                            IPAddress.Parse(plcDevice.IpAddress),
                            502));
                });

                if (!modbusClient.IsConnected)
                {
                    Debug.WriteLine(
                        $"Failed to connect to PLC at {plcDevice.IpAddress}");

                    return false;
                }

                const int unitIdentifier = 1;

                if (tag.Type == DataType.BOOL)
                {
                    // BOOL = Coil
                    await Task.Run(() =>
                    {
                        modbusClient.WriteSingleCoil(
                            unitIdentifier,
                            tag.Address,
                            value);
                    });
                }
                else
                {
                    // WBOOL = Bit inside holding register
                    await Task.Run(() =>
                    {
                        WriteWBoolBit(
                            modbusClient,
                            unitIdentifier,
                            tag.Address,
                            tag.Bit,
                            value);
                    });
                }

                Debug.WriteLine(
                    $"PLC BOOL WRITE SUCCESS: " +
                    $"Tag={tag.Name}, " +
                    $"Address={tag.Address}, " +
                    $"Bit={tag.Bit}, " +
                    $"Value={value}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"WriteBoolAsync failed: {ex}");

                return false;
            }
        }
        private void WriteWBoolBit(
    ModbusTcpClient modbusClient,
    int unitIdentifier,
    int registerAddress,
    int bitIndex,
    bool value)
        {
            if (bitIndex < 0 || bitIndex > 15)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bitIndex),
                    "Bit index must be between 0 and 15.");
            }

            // Read current register value
            ushort rawValue =
                modbusClient
                    .ReadHoldingRegisters<ushort>(
                        unitIdentifier,
                        registerAddress,
                        1)
                    .ToArray()[0];

            // PLC uses byte-swapped register order.
            ushort currentValue =
                (ushort)(
                    ((rawValue & 0x00FF) << 8) |
                    (rawValue >> 8));

            // Change only the requested bit.
            ushort newValue;

            if (value)
            {
                newValue =
                    (ushort)(
                        currentValue |
                        (1 << bitIndex));
            }
            else
            {
                newValue =
                    (ushort)(
                        currentValue &
                        ~(1 << bitIndex));
            }

            // Nothing changed.
            if (newValue == currentValue)
                return;

            // Swap bytes back before writing to PLC.
            ushort writeValue =
                (ushort)(
                    ((newValue & 0x00FF) << 8) |
                    (newValue >> 8));

            modbusClient.WriteSingleRegister(
                unitIdentifier,
                registerAddress,
                writeValue);
        }

        public async Task<bool> WriteRealAsync(int tagId, float value)
        {
            try
            {
                // Get PLC device
                var plcDevice = _repository?
                    .GetIODevices()
                    .FirstOrDefault(d =>
                        d.DeviceType == IODeviceType.PLC ||
                        d.DeviceType == IODeviceType.EthernetDevice);

                if (plcDevice == null ||
                    string.IsNullOrWhiteSpace(plcDevice.IpAddress))
                {
                    Debug.WriteLine(
                        "PLC device not found or IP address not configured.");

                    return false;
                }

                // Get the tag
                var tag = _repository?
                    .GetTags()
                    .FirstOrDefault(t =>
                        t.Id == tagId &&
                        t.DeviceId == plcDevice.Id);

                if (tag == null)
                {
                    Debug.WriteLine(
                        $"PLC tag not found. TagId={tagId}");

                    return false;
                }

                // Make sure this is a REAL tag
                if (tag.Type != DataType.REAL)
                {
                    Debug.WriteLine(
                        $"Tag {tag.Name} is {tag.Type}, not REAL.");

                    return false;
                }

                return await WriteRealToPLCAsync(
                    plcDevice,
                    tag.Address,
                    value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"WriteRealAsync failed: {ex}");

                return false;
            }
        }


        private async Task<bool> WriteRealToPLCAsync(
            IODevice plcDevice,
            int registerAddress,
            float value)
        {
            using var modbusClient = new ModbusTcpClient();

            try
            {
                Debug.WriteLine(
                    $"PLC WRITE: IP={plcDevice.IpAddress}, " +
                    $"Address={registerAddress}, " +
                    $"Value={value}");

                await Task.Run(() =>
                {
                    modbusClient.Connect(
                        new IPEndPoint(
                            IPAddress.Parse(plcDevice.IpAddress),
                            502));
                });

                if (!modbusClient.IsConnected)
                {
                    Debug.WriteLine(
                        "Failed to connect to PLC.");

                    return false;
                }

                const int unitIdentifier = 1;

                // REAL = 32-bit IEEE-754 = 2 registers
                byte[] bytes =
                    BitConverter.GetBytes(value);

                // Your existing REAL READ expects:
                //
                // Register 1 -> bytes [0], [1]
                // Register 2 -> bytes [2], [3]
                //
                // with byte order handled per register.

                ushort word1 =
                    BitConverter.ToUInt16(bytes, 0);

                ushort word2 =
                    BitConverter.ToUInt16(bytes, 2);

                // Swap bytes in each 16-bit register.
                ushort register1 =
                    (ushort)(
                        (word1 >> 8) |
                        (word1 << 8));

                ushort register2 =
                    (ushort)(
                        (word2 >> 8) |
                        (word2 << 8));

                await Task.Run(() =>
                {
                    // REAL occupies two consecutive holding registers.
                    modbusClient.WriteSingleRegister(
                        unitIdentifier,
                        registerAddress,
                        register1);

                    modbusClient.WriteSingleRegister(
                        unitIdentifier,
                        registerAddress + 1,
                        register2);
                });

                Debug.WriteLine(
                    $"PLC REAL WRITE SUCCESS: " +
                    $"Address={registerAddress}, " +
                    $"Value={value}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"PLC REAL WRITE ERROR: {ex}");

                return false;
            }
            finally
            {
                if (modbusClient.IsConnected)
                    modbusClient.Disconnect();
            }
        }

        // ============================================================
        // COIL BLOCKS
        // ============================================================

        private static List<(int StartAddress,
                             int Count,
                             List<Tag> Tags)>
            BuildCoilBlocks(
                List<Tag> tags,
                int maxGapCoils = 16,
                int maxBlockCoils = 500)
        {
            var blocks =
                new List<(int, int, List<Tag>)>();

            var sorted =
                tags
                    .Where(t => t.Type == DataType.BOOL)
                    .OrderBy(t => t.Address)
                    .ToList();

            int start = -1;
            int count = 0;

            List<Tag> current = null;

            foreach (var tag in sorted)
            {
                if (current == null ||
                    tag.Address - (start + count) > maxGapCoils ||
                    tag.Address - start + 1 > maxBlockCoils)
                {
                    start = tag.Address;
                    count = 1;

                    current = new List<Tag>();

                    blocks.Add(
                        (start, count, current));
                }
                else
                {
                    count =
                        tag.Address - start + 1;

                    blocks[^1] =
                        (start, count, current);
                }

                current.Add(tag);
            }

            return blocks;
        }

        // ============================================================
        // VALUE DECODER
        // ============================================================

        private static double DecodeValue(
            byte[] buffer,
            int offset,
            DataType type,
            bool swapRegs = false)
        {
            switch (type)
            {
                case DataType.INT:

                    return BitConverter.ToInt16(
                        new[]
                        {
                            buffer[offset + 1],
                            buffer[offset]
                        },
                        0);

                case DataType.UINT:
                case DataType.WBOOL:

                    return BitConverter.ToUInt16(
                        new[]
                        {
                            buffer[offset + 1],
                            buffer[offset]
                        },
                        0);

                case DataType.DINT:

                    return BitConverter.ToInt32(
                        new[]
                        {
                            buffer[offset + 3],
                            buffer[offset + 2],
                            buffer[offset + 1],
                            buffer[offset]
                        },
                        0);

                case DataType.UDINT:

                    return BitConverter.ToUInt32(
                        new[]
                        {
                            buffer[offset + 3],
                            buffer[offset + 2],
                            buffer[offset + 1],
                            buffer[offset]
                        },
                        0);

                case DataType.REAL:

                    return swapRegs
                        ? BitConverter.ToSingle(
                            new[]
                            {
                                buffer[offset + 3],
                                buffer[offset + 2],
                                buffer[offset + 1],
                                buffer[offset]
                            },
                            0)

                        : BitConverter.ToSingle(
                            new[]
                            {
                                buffer[offset + 1],
                                buffer[offset],
                                buffer[offset + 3],
                                buffer[offset + 2]
                            },
                            0);

                case DataType.INT64:
                    {
                        float hi =
                            BitConverter.ToSingle(
                                new[]
                                {
                                buffer[offset + 7],
                                buffer[offset + 6],
                                buffer[offset + 5],
                                buffer[offset + 4]
                                },
                                0);

                        float lo =
                            BitConverter.ToSingle(
                                new[]
                                {
                                buffer[offset + 1],
                                buffer[offset],
                                buffer[offset + 3],
                                buffer[offset + 2]
                                },
                                0);

                        return
                            hi * 4294967.296 +
                            (lo / 1000.0);
                    }

                default:

                    throw new NotSupportedException(
                        $"DecodeValue: unsupported {type}");
            }
        }

        // ============================================================
        // REGISTER COUNT
        // ============================================================

        private static int GetRegisterCount(
            DataType type)
        {
            return type switch
            {
                DataType.INT => 1,
                DataType.UINT => 1,
                DataType.WBOOL => 1,
                DataType.DINT => 2,
                DataType.UDINT => 2,
                DataType.REAL => 2,
                DataType.INT64 => 4,
                _ => 1
            };
        }

        // ============================================================
        // REGISTER BLOCKS
        // ============================================================

        private class RegisterBlock
        {
            public int StartAddress { get; set; }

            public int RegisterCount { get; set; }

            public List<Tag> Tags { get; set; }
                = new();
        }

        private static List<RegisterBlock>
            BuildRegisterBlocks(
                List<Tag> tags,
                int maxGapRegisters = 10,
                int maxBlockRegisters = 120)
        {
            var blocks =
                new List<RegisterBlock>();

            var sorted =
                tags
                    .Where(t => t.Type != DataType.BOOL)
                    .OrderBy(t => t.Address)
                    .ToList();

            RegisterBlock current = null;

            foreach (var tag in sorted)
            {
                int registerCount =
                    GetRegisterCount(tag.Type);

                int tagEndExclusive =
                    tag.Address + registerCount;

                if (current == null ||
                    tag.Address -
                    (current.StartAddress +
                     current.RegisterCount)
                    > maxGapRegisters ||
                    tagEndExclusive -
                    current.StartAddress
                    > maxBlockRegisters)
                {
                    current =
                        new RegisterBlock
                        {
                            StartAddress =
                                tag.Address,

                            RegisterCount =
                                registerCount
                        };

                    blocks.Add(current);
                }
                else
                {
                    current.RegisterCount =
                        Math.Max(
                            current.RegisterCount,
                            tagEndExclusive -
                            current.StartAddress);
                }

                current.Tags.Add(tag);
            }

            return blocks;
        }

    }
}