using System;
using System.Collections;
using System.Windows.Forms;

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
        protected string ConnectionString { get; set; }

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
        }

        public ModbusUtility(int connectionType, string connectionString)
        {
            ConnectionString = connectionString;
            ModbusType = (ModbusType)connectionType;           
        }

        public override void SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public override void SetConnectionType(int connectionType)
        {
            ModbusType = (ModbusType) connectionType;
        }

        public override byte[] GetDatas(byte belongAddress, byte functionCode, string startAddress, ushort getCount)
        {
            try
            {
                var inputStruct = new ReadDataInputStruct(belongAddress, (ModbusProtocalReadDataFunctionCode)functionCode, startAddress, getCount);
                var outputStruct =
                    Wrapper.SendReceive(Wrapper["ReadDataModbusProtocal"], inputStruct) as ReadDataOutputStruct;
                return outputStruct.DataValue;
            }
            catch
            {
                return null;
            }    
        }

        public override bool SetDatas(byte belongAddress, byte functionCode, string startAddress, object[] setContents)
        {
            try
            {
                var inputStruct = new WriteDataInputStruct(belongAddress,
                    (ModbusProtocalWriteDataFunctionCode) functionCode, startAddress, setContents);
                var outputStruct =
                    Wrapper.SendReceive(Wrapper["WriteDataModbusProtocal"], inputStruct) as
                        WriteDataOutputStruct;
                if (outputStruct.WriteCount != setContents.Length) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
