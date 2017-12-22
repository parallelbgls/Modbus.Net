using System;
using System.Threading.Tasks;
using Serilog;

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
    }

    /// <summary>
    ///     写单个单元的方法接口
    /// </summary>
    public interface IUtilityMethodWriteSingle : IUtilityMethod
    {
        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="setContent">需要设置的数据</param>
        /// <returns>设置是否成功</returns>
        Task<bool> SetSingleDataAsync(string startAddress, object setContent);
    }

    /// <summary>
    ///     Modbus基础Api入口
    /// </summary>
    public class ModbusUtility : BaseUtility, IUtilityMethodTime, IUtilityMethodWriteSingle
    {
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
            Endian endian = Endian.BigEndianLsb)
            : base(slaveAddress, masterAddress)
        {
            Endian = endian;
            ConnectionString = null;
            ModbusType = (ModbusType) connectionType;
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
            Endian endian = Endian.BigEndianLsb)
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
                    return connectionStringSplit.Length < 2 ? (int?) null : int.Parse(connectionStringSplit[1]);
                }
                catch (Exception e)
                {
                    Log.Error(e, $"ModbusUtility: {ConnectionString} format error");
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
                            ? new ModbusRtuProtocol(SlaveAddress, MasterAddress, Endian)
                            : new ModbusRtuProtocol(ConnectionString, SlaveAddress, MasterAddress, Endian);
                        break;
                    }
                    //Tcp协议
                    case ModbusType.Tcp:
                    {
                        Wrapper = ConnectionString == null
                            ? new ModbusTcpProtocol(SlaveAddress, MasterAddress, Endian)
                            : (ConnectionStringPort == null
                                ? new ModbusTcpProtocol(ConnectionString, SlaveAddress, MasterAddress, Endian)
                                : new ModbusTcpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                    MasterAddress, Endian));
                        break;
                    }
                    //Ascii协议                    
                    case ModbusType.Ascii:
                    {
                        Wrapper = ConnectionString == null
                            ? new ModbusAsciiProtocol(SlaveAddress, MasterAddress, Endian)
                            : new ModbusAsciiProtocol(ConnectionString, SlaveAddress, MasterAddress, Endian);
                        break;
                    }
                    //Rtu协议
                    case ModbusType.RtuInTcp:
                    {
                        Wrapper = ConnectionString == null
                            ? new ModbusRtuInTcpProtocol(SlaveAddress, MasterAddress, Endian)
                            : (ConnectionStringPort == null
                                ? new ModbusRtuInTcpProtocol(ConnectionString, SlaveAddress, MasterAddress, Endian)
                                : new ModbusRtuInTcpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                    MasterAddress, Endian));
                        break;
                    }
                    //Ascii协议
                    case ModbusType.AsciiInTcp:
                    {
                        Wrapper = ConnectionString == null
                            ? new ModbusAsciiInTcpProtocol(SlaveAddress, MasterAddress, Endian)
                            : (ConnectionStringPort == null
                                ? new ModbusAsciiInTcpProtocol(ConnectionString, SlaveAddress, MasterAddress, Endian)
                                : new ModbusAsciiInTcpProtocol(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                    MasterAddress, Endian));
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     读时间
        /// </summary>
        /// <returns>设备的时间</returns>
        public async Task<DateTime> GetTimeAsync()
        {
            try
            {
                var inputStruct = new GetSystemTimeModbusInputStruct(SlaveAddress);
                var outputStruct =
                    await Wrapper.SendReceiveAsync<GetSystemTimeModbusOutputStruct>(
                        Wrapper[typeof(GetSystemTimeModbusProtocol)], inputStruct);
                return outputStruct?.Time ?? DateTime.MinValue;
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> GetTime: {ConnectionString} error");
                return DateTime.MinValue;
            }
        }

        /// <summary>
        ///     写时间
        /// </summary>
        /// <param name="setTime">需要写入的时间</param>
        /// <returns>写入是否成功</returns>
        public async Task<bool> SetTimeAsync(DateTime setTime)
        {
            try
            {
                var inputStruct = new SetSystemTimeModbusInputStruct(SlaveAddress, setTime);
                var outputStruct =
                    await Wrapper.SendReceiveAsync<SetSystemTimeModbusOutputStruct>(
                        Wrapper[typeof(SetSystemTimeModbusProtocol)], inputStruct);
                return outputStruct?.WriteCount > 0;
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> SetTime: {ConnectionString} error");
                return false;
            }
        }

        /// <summary>
        ///     设置协议类型
        /// </summary>
        /// <param name="connectionType">协议类型</param>
        public override void SetConnectionType(int connectionType)
        {
            ModbusType = (ModbusType) connectionType;
        }

        /// <summary>
        ///     读数据
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="getByteCount">获取字节个数</param>
        /// <returns>获取的结果</returns>
        public override async Task<byte[]> GetDatasAsync(string startAddress, int getByteCount)
        {
            try
            {
                var inputStruct = new ReadDataModbusInputStruct(SlaveAddress, startAddress,
                    (ushort) getByteCount, AddressTranslator);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<ReadDataModbusOutputStruct>(Wrapper[typeof(ReadDataModbusProtocol)],
                        inputStruct);
                return outputStruct?.DataValue;
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> GetDatas: {ConnectionString} error");
                return null;
            }
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="setContents">需要设置的数据</param>
        /// <returns>设置是否成功</returns>
        public override async Task<bool> SetDatasAsync(string startAddress, object[] setContents)
        {
            try
            {
                var inputStruct = new WriteDataModbusInputStruct(SlaveAddress, startAddress, setContents,
                    AddressTranslator, Endian);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<WriteDataModbusOutputStruct>(Wrapper[typeof(WriteDataModbusProtocol)],
                        inputStruct);
                return outputStruct?.WriteCount == setContents.Length;
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> SetDatas: {ConnectionString} error");
                return false;
            }
        }

        /// <summary>
        ///     写数据
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="setContent">需要设置的数据</param>
        /// <returns>设置是否成功</returns>
        public async Task<bool> SetSingleDataAsync(string startAddress, object setContent)
        {
            try
            {
                var inputStruct = new WriteSingleDataModbusInputStruct(SlaveAddress, startAddress, setContent,
                    (ModbusTranslatorBase)AddressTranslator, Endian);
                var outputStruct = await
                    Wrapper.SendReceiveAsync<WriteSingleDataModbusOutputStruct>(Wrapper[typeof(WriteSingleDataModbusProtocol)],
                        inputStruct);
                return outputStruct?.WriteValue.ToString() == setContent.ToString();
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> SetSingleDatas: {ConnectionString} error");
                return false;
            }
        }
    }
}