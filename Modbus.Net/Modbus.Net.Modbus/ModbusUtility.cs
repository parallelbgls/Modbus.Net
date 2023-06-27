using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus连接类型
    /// </summary>
    public enum ModbusType
    {
        /// <summary>
        ///     Rtu连接
        /// </summary>
        Rtu = 0,

        /// <summary>
        ///     Tcp连接
        /// </summary>
        Tcp = 1,

        /// <summary>
        ///     Ascii连接
        /// </summary>
        Ascii = 2,

        /// <summary>
        ///     Rtu连接Tcp透传
        /// </summary>
        RtuInTcp = 3,

        /// <summary>
        ///     Ascii连接Tcp透传
        /// </summary>
        AsciiInTcp = 4,

        /// <summary>
        ///     Udp连接
        /// </summary>
        Udp = 5,

        /// <summary>
        ///     Rtu连接Udp透传
        /// </summary>
        RtuInUdp = 6,

        /// <summary>
        ///     Ascii连接Udp透传
        /// </summary>
        AsciiInUdp = 7
    }

    /// <summary>
    ///     Modbus基础Api入口
    /// </summary>
    public class ModbusUtility : BaseUtility<byte[], byte[], ProtocolUnit<byte[], byte[]>, PipeUnit>,
        IUtilityMethodExceptionStatus,
        IUtilityMethodDiagnotics,
        IUtilityMethodCommEventCounter,
        IUtilityMethodCommEventLog,
        IUtilityMethodSlaveId,
        IUtilityMethodFileRecord,
        IUtilityMethodMaskRegister,
        IUtilityMethodMultipleRegister,
        IUtilityMethodFIFOQueue
    {
        private static readonly ILogger<ModbusUtility> logger = LogProvider.CreateLogger<ModbusUtility>();

        /// <summary>
        ///     Modbus协议类型
        /// </summary>
        private ModbusType _modbusType;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionType">协议类型</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusUtility(int connectionType, byte slaveAddress, byte masterAddress,
            Endian endian)
            : base(slaveAddress, masterAddress)
        {
            Endian = endian;
            ConnectionString = null;
            ModbusType = (ModbusType)connectionType;
            AddressTranslator = new AddressTranslatorModbus();
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionType">协议类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusUtility(ModbusType connectionType, string connectionString, byte slaveAddress, byte masterAddress,
            Endian endian)
            : base(slaveAddress, masterAddress)
        {
            Endian = endian;
            ConnectionString = connectionString;
            ModbusType = connectionType;
            AddressTranslator = new AddressTranslatorModbus();
        }

        /// <summary>
        ///     端格式
        /// </summary>
        public override Endian Endian { get; }

        /// <summary>
        ///     Ip地址
        /// </summary>
        protected string ConnectionStringIp
        {
            get
            {
                if (ConnectionString == null) return null;
                return ConnectionString.Contains(":") ? ConnectionString.Split(':')[0] : ConnectionString;
            }
        }

        /// <summary>
        ///     端口
        /// </summary>
        protected int? ConnectionStringPort
        {
            get
            {
                if (ConnectionString == null) return null;
                if (!ConnectionString.Contains(":")) return null;
                var connectionStringSplit = ConnectionString.Split(':');
                try
                {
                    return connectionStringSplit.Length < 2 ? (int?)null : int.Parse(connectionStringSplit[1]);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"ModbusUtility: {ConnectionString} format error");
                    return null;
                }
            }
        }

        /// <summary>
        ///     协议类型
        /// </summary>
        public ModbusType ModbusType
        {
            get { return _modbusType; }
            set
            {
                _modbusType = value;
                switch (_modbusType)
                {
                    //Rtu协议
                    case ModbusType.Rtu:
                        {
                            Wrapper = ConnectionString == null
                                ? new ModbusRtuProtocol(SlaveAddress, MasterAddress)
                                : new ModbusRtuProtocol(ConnectionString, SlaveAddress, MasterAddress);
                            break;
                        }
                    //Tcp协议
                    case ModbusType.Tcp:
                        {
                            Wrapper = ConnectionString == null
                                ? new ModbusTcpProtocol(SlaveAddress, MasterAddress)
                                : (ConnectionStringPort == null
                                    ? new ModbusTcpProtocol(ConnectionString, SlaveAddress, MasterAddress)
                                    : new ModbusTcpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                        MasterAddress));
                            break;
                        }
                    //Ascii协议                    
                    case ModbusType.Ascii:
                        {
                            Wrapper = ConnectionString == null
                                ? new ModbusAsciiProtocol(SlaveAddress, MasterAddress)
                                : new ModbusAsciiProtocol(ConnectionString, SlaveAddress, MasterAddress);
                            break;
                        }
                    //Rtu协议Tcp透传
                    case ModbusType.RtuInTcp:
                        {
                            Wrapper = ConnectionString == null
                                ? new ModbusRtuInTcpProtocol(SlaveAddress, MasterAddress)
                                : (ConnectionStringPort == null
                                    ? new ModbusRtuInTcpProtocol(ConnectionString, SlaveAddress, MasterAddress)
                                    : new ModbusRtuInTcpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                        MasterAddress));
                            break;
                        }
                    //Ascii协议Tcp透传
                    case ModbusType.AsciiInTcp:
                        {
                            Wrapper = ConnectionString == null
                                ? new ModbusAsciiInTcpProtocol(SlaveAddress, MasterAddress)
                                : (ConnectionStringPort == null
                                    ? new ModbusAsciiInTcpProtocol(ConnectionString, SlaveAddress, MasterAddress)
                                    : new ModbusAsciiInTcpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                        MasterAddress));
                            break;
                        }
                    //Tcp协议Udp透传
                    case ModbusType.Udp:
                        {
                            Wrapper = ConnectionString == null
                                ? new ModbusUdpProtocol(SlaveAddress, MasterAddress)
                                : (ConnectionStringPort == null
                                    ? new ModbusUdpProtocol(ConnectionString, SlaveAddress, MasterAddress)
                                    : new ModbusUdpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                        MasterAddress));
                            break;
                        }
                    //Rtu协议Udp透传
                    case ModbusType.RtuInUdp:
                        {
                            Wrapper = ConnectionString == null
                                ? new ModbusRtuInUdpProtocol(SlaveAddress, MasterAddress)
                                : (ConnectionStringPort == null
                                    ? new ModbusRtuInUdpProtocol(ConnectionString, SlaveAddress, MasterAddress)
                                    : new ModbusRtuInUdpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                        MasterAddress));
                            break;
                        }
                    //Rtu协议Udp透传
                    case ModbusType.AsciiInUdp:
                        {
                            Wrapper = ConnectionString == null
                                ? new ModbusAsciiInUdpProtocol(SlaveAddress, MasterAddress)
                                : (ConnectionStringPort == null
                                    ? new ModbusAsciiInUdpProtocol(ConnectionString, SlaveAddress, MasterAddress)
                                    : new ModbusAsciiInUdpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                        MasterAddress));
                            break;
                        }
                }
            }
        }

        /// <summary>
        ///     设置协议类型
        /// </summary>
        /// <param name="connectionType">协议类型</param>
        public override void SetConnectionType(int connectionType)
        {
            ModbusType = (ModbusType)connectionType;
        }

        /// <inheritdoc />
        public override async Task<ReturnStruct<byte[]>> GetDatasAsync(string startAddress, int getByteCount)
        {
            try
            {
                var inputStruct = new ReadDataModbusInputStruct(SlaveAddress, startAddress,
                    (ushort)getByteCount, AddressTranslator);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<ReadDataModbusOutputStruct>(Wrapper[typeof(ReadDataModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<byte[]>
                {
                    Datas = outputStruct?.DataValue,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = ""
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> GetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<byte[]>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public override async Task<ReturnStruct<bool>> SetDatasAsync(string startAddress, object[] setContents)
        {
            try
            {
                var inputStruct = new WriteDataModbusInputStruct(SlaveAddress, startAddress, setContents,
                    AddressTranslator, Endian);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<WriteDataModbusOutputStruct>(Wrapper[typeof(WriteDataModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<bool>()
                {
                    Datas = outputStruct?.WriteCount == setContents.Length,
                    IsSuccess = outputStruct?.WriteCount == setContents.Length,
                    ErrorCode = outputStruct?.WriteCount == setContents.Length ? 0 : -2,
                    ErrorMsg = outputStruct?.WriteCount == setContents.Length ? "" : "Data length mismatch"
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<bool>
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<byte>> GetExceptionStatusAsync()
        {
            try
            {
                var inputStruct = new ReadExceptionStatusModbusInputStruct(SlaveAddress);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<ReadExceptionStatusModbusOutputStruct>(Wrapper[typeof(ReadExceptionStatusModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<byte>()
                {
                    Datas = outputStruct.OutputData,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<byte>
                {
                    Datas = 0,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<DiagnoticsData>> GetDiagnoticsAsync(ushort subFunction, ushort[] data)
        {
            try
            {
                var inputStruct = new DiagnoticsModbusInputStruct(SlaveAddress, subFunction, data);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<DiagnoticsModbusOutputStruct>(Wrapper[typeof(DiagnoticsModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<DiagnoticsData>()
                {
                    Datas = new DiagnoticsData() { SubFunction = outputStruct.SubFunction, Data = outputStruct.Data },
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<DiagnoticsData>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<CommEventCounterData>> GetCommEventCounterAsync()
        {
            try
            {
                var inputStruct = new GetCommEventCounterModbusInputStruct(SlaveAddress);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<GetCommEventCounterModbusOutputStruct>(Wrapper[typeof(GetCommEventCounterModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<CommEventCounterData>()
                {
                    Datas = new CommEventCounterData() { EventCount = outputStruct.EventCount, Status = outputStruct.Status },
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<CommEventCounterData>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<CommEventLogData>> GetCommEventLogAsync()
        {
            try
            {
                var inputStruct = new GetCommEventLogModbusInputStruct(SlaveAddress);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<GetCommEventLogModbusOutputStruct>(Wrapper[typeof(GetCommEventLogModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<CommEventLogData>()
                {
                    Datas = new CommEventLogData() { Status = outputStruct.Status, Events = outputStruct.Events },
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<CommEventLogData>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<SlaveIdData>> GetSlaveIdAsync()
        {
            try
            {
                var inputStruct = new ReportSlaveIdModbusInputStruct(SlaveAddress);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<ReportSlaveIdModbusOutputStruct>(Wrapper[typeof(ReportSlaveIdModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<SlaveIdData>()
                {
                    Datas = new SlaveIdData() { SlaveId = outputStruct.SlaveId, IndicatorStatus = outputStruct.RunIndicatorStatus, AdditionalData = outputStruct.AdditionalData },
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<SlaveIdData>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<ReadFileRecordOutputDef[]>> GetFileRecordAsync(ReadFileRecordInputDef[] recordDefs)
        {
            try
            {
                var inputStruct = new ReadFileRecordModbusInputStruct(SlaveAddress, recordDefs);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<ReadFileRecordModbusOutputStruct>(Wrapper[typeof(ReadFileRecordModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<ReadFileRecordOutputDef[]>()
                {
                    Datas = outputStruct.RecordDefs,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<ReadFileRecordOutputDef[]>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<WriteFileRecordOutputDef[]>> SetFileRecordAsync(WriteFileRecordInputDef[] recordDefs)
        {
            try
            {
                var inputStruct = new WriteFileRecordModbusInputStruct(SlaveAddress, recordDefs);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<WriteFileRecordModbusOutputStruct>(Wrapper[typeof(WriteFileRecordModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<WriteFileRecordOutputDef[]>()
                {
                    Datas = outputStruct.WriteRecords,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<WriteFileRecordOutputDef[]>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<MaskRegisterData>> SetMaskRegister(ushort referenceAddress, ushort andMask, ushort orMask)
        {
            try
            {
                var inputStruct = new MaskWriteRegisterModbusInputStruct(SlaveAddress, referenceAddress, andMask, orMask);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<MaskWriteRegisterModbusOutputStruct>(Wrapper[typeof(MaskWriteRegisterModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<MaskRegisterData>()
                {
                    Datas = new MaskRegisterData() { ReferenceAddress = outputStruct.ReferenceAddress, AndMask = outputStruct.AndMask, OrMask = outputStruct.OrMask },
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<MaskRegisterData>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<ushort[]>> GetMultipleRegister(ushort readStartingAddress, ushort quantityToRead, ushort writeStartingAddress, ushort[] writeValues)
        {
            try
            {
                var inputStruct = new ReadWriteMultipleRegistersModbusInputStruct(SlaveAddress, readStartingAddress, quantityToRead, writeStartingAddress, writeValues);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<ReadWriteMultipleRegistersModbusOutputStruct>(Wrapper[typeof(ReadWriteMultipleRegistersModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<ushort[]>()
                {
                    Datas = outputStruct.ReadRegisterValues,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<ushort[]>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<ReturnStruct<ushort[]>> GetFIFOQueue(ushort fifoPointerAddress)
        {
            try
            {
                var inputStruct = new ReadFIFOQueueModbusInputStruct(SlaveAddress, fifoPointerAddress);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<ReadFIFOQueueModbusOutputStruct>(Wrapper[typeof(ReadFIFOQueueModbusProtocol)],
                        inputStruct);
                return new ReturnStruct<ushort[]>()
                {
                    Datas = outputStruct.FIFOValueRegister,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = null
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<ushort[]>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }
    }
}