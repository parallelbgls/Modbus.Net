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
        Ascii = 2
    }

    /// <summary>
    ///     Modbus基础Api入口
    /// </summary>
    public class ModbusUtility : BaseUtility
    {
        /// <summary>
        ///     Modbus协议类型
        /// </summary>
        private ModbusType _modbusType;

        public ModbusUtility(int connectionType, byte slaveAddress, byte masterAddress, Endian endian = Endian.BigEndianLsb)
            : base(slaveAddress, masterAddress)
        {
            Endian = endian;
            ConnectionString = null;
            ModbusType = (ModbusType) connectionType;
            AddressTranslator = new AddressTranslatorModbus();
        }

        public ModbusUtility(ModbusType connectionType, string connectionString, byte slaveAddress, byte masterAddress, Endian endian = Endian.BigEndianLsb)
            : base(slaveAddress, masterAddress)
        {
            Endian = endian;
            ConnectionString = connectionString;
            ModbusType = connectionType;
            AddressTranslator = new AddressTranslatorModbus();
        }

        public override Endian Endian { get; }

        protected string ConnectionStringIp
        {
            get
            {
                if (ConnectionString == null) return null;
                return ConnectionString.Contains(":") ? ConnectionString.Split(':')[0] : ConnectionString;
            }
        }

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
                catch
                {
                    return null;
                }
            }
        }

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
                            ? new ModbusRtuProtocal(SlaveAddress, MasterAddress, Endian)
                            : new ModbusRtuProtocal(ConnectionString, SlaveAddress, MasterAddress, Endian);
                        break;
                    }
                    //Tcp协议
                    case ModbusType.Tcp:
                    {
                        Wrapper = ConnectionString == null
                            ? new ModbusTcpProtocal(SlaveAddress, MasterAddress, Endian)
                            : (ConnectionStringPort == null
                                ? new ModbusTcpProtocal(ConnectionString, SlaveAddress, MasterAddress, Endian)
                                : new ModbusTcpProtocal(ConnectionStringIp, ConnectionStringPort.Value, SlaveAddress,
                                    MasterAddress, Endian));
                        break;
                    }
                    //Ascii协议
                    case ModbusType.Ascii:
                    {
                        Wrapper = ConnectionString == null
                            ? new ModbusAsciiProtocal(SlaveAddress, MasterAddress, Endian)
                            : new ModbusAsciiProtocal(ConnectionString, SlaveAddress, MasterAddress, Endian);
                        break;
                    }
                }
            }
        }

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
                    Wrapper.SendReceiveAsync(Wrapper[typeof (ReadDataModbusProtocal)], inputStruct) as
                    ReadDataModbusOutputStruct;
                return outputStruct?.DataValue;
            }
            catch
            {
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
                    Wrapper.SendReceiveAsync(Wrapper[typeof (WriteDataModbusProtocal)], inputStruct) as
                    WriteDataModbusOutputStruct;
                return outputStruct?.WriteCount == setContents.Length;
            }
            catch
            {
                return false;
            }
        }

        /*
        /// <summary>
        ///     读时间
        /// </summary>
        /// <returns>设备的时间</returns>
        public override DateTime GetTime()
        {
            try
            {
                var inputStruct = new GetSystemTimeModbusInputStruct(SlaveAddress);
                var outputStruct =
                    Wrapper.SendReceive(Wrapper[typeof(GetSystemTimeModbusProtocal)], inputStruct) as
                        GetSystemTimeModbusOutputStruct;
                return outputStruct?.Time;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        ///     写时间
        /// </summary>
        /// <param name="setTime">需要写入的时间</param>
        /// <returns>写入是否成功</returns>
        public override bool SetTime(DateTime setTime)
        {
            try
            {
                var inputStruct = new SetSystemTimeModbusInputStruct(SlaveAddress, setTime);
                var outputStruct =
                    Wrapper.SendReceive(Wrapper[typeof(SetSystemTimeModbusProtocal)], inputStruct) as
                        SetSystemTimeModbusOutputStruct;
                return outputStruct?.WriteCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        */
    }
}