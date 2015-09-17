using System;
using System.Threading.Tasks;

/// <summary>
/// Modbus连接类型
/// </summary>
public enum ModbusType
{
    /// <summary>
    /// Rtu连接
    /// </summary>
    Rtu = 0,
    /// <summary>
    /// Tcp连接
    /// </summary>
    Tcp = 1,
}

namespace ModBus.Net
{
    public class ModbusUtility : BaseUtility
    {
        private ModbusType _modbusType;

        public ModbusType ModbusType
        {
            get
            {
                return _modbusType;
            }
            set
            {
                _modbusType = value;
                switch (_modbusType)
                {
                    case ModbusType.Rtu:
                    {
                        Wrapper = ConnectionString == null ? new ModbusRtuProtocal() : new ModbusRtuProtocal(ConnectionString);
                        break;
                    }
                    case ModbusType.Tcp:
                    {
                        Wrapper = ConnectionString == null ? new ModbusTcpProtocal() : new ModbusTcpProtocal(ConnectionString);
                        break;
                    }
                }
            }
        }

        public ModbusUtility(int connectionType)
        {
            ConnectionString = null;
            ModbusType = (ModbusType)connectionType;
            AddressTranslator = new AddressTranslatorBase();
        }

        public ModbusUtility(ModbusType connectionType, string connectionString)
        {
            ConnectionString = connectionString;
            ModbusType = connectionType;
            AddressTranslator = new AddressTranslatorBase();
        }

        public override void SetConnectionType(int connectionType)
        {
            ModbusType = (ModbusType) connectionType;
        }

        protected override async Task<byte[]> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress, int getByteCount)
        {
            try
            {
                var inputStruct = new ReadDataModbusInputStruct(belongAddress, startAddress, getByteCount % 2 == 0 ? (ushort)(getByteCount / 2) : (ushort)(getByteCount / 2 + 1), AddressTranslator);
                var outputStruct = await 
                    Wrapper.SendReceiveAsync(Wrapper[typeof(ReadDataModbusProtocal)], inputStruct) as ReadDataModbusOutputStruct;
                return outputStruct.DataValue;
            }
            catch
            {
                return null;
            }
        }

        public override async Task<bool> SetDatasAsync(byte belongAddress, byte masterAddress, string startAddress, object[] setContents)
        {
            try
            {
                var inputStruct = new WriteDataModbusInputStruct(belongAddress, startAddress, setContents, AddressTranslator);
                var outputStruct = await
                    Wrapper.SendReceiveAsync(Wrapper[typeof(WriteDataModbusProtocal)], inputStruct) as
                        WriteDataModbusOutputStruct;
                if (outputStruct.WriteCount != setContents.Length) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /*
        public override DateTime GetTime(byte belongAddress)
        {
            try
            {
                var inputStruct = new GetSystemTimeModbusInputStruct(belongAddress);
                var outputStruct =
                    Wrapper.SendReceive(Wrapper[typeof(GetSystemTimeModbusProtocal)], inputStruct) as
                        GetSystemTimeModbusOutputStruct;
                return outputStruct.Time;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        public override bool SetTime(byte belongAddress, DateTime setTime)
        {
            try
            {
                var inputStruct = new SetSystemTimeModbusInputStruct(belongAddress, setTime);
                var outputStruct =
                    Wrapper.SendReceive(Wrapper[typeof(SetSystemTimeModbusProtocal)], inputStruct) as
                        SetSystemTimeModbusOutputStruct;
                return outputStruct.WriteCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        */
    }
}
