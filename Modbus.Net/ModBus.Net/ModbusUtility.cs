using System;
using System.Collections;
using System.Windows.Forms;

public enum ModbusType
{
    Rtu = 0,
    Tcp = 1,
}

namespace ModBus.Net
{
    public class ModbusUtility : BaseUtility
    {
        private BaseProtocal _wrapper;

        private string _connectionString;

        public string ConnectionString { get; set; }

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
                        _wrapper = ConnectionString == null ? new ModbusRtuProtocal() : new ModbusRtuProtocal(ConnectionString);
                        break;
                    }
                    case ModbusType.Tcp:
                    {
                        _wrapper = ConnectionString == null ? new ModbusTcpProtocal() : new ModbusTcpProtocal(ConnectionString);
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

        public ModbusUtility(string connectionString, int connectionType)
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

        public override bool[] GetCoils(byte belongAddress, string startAddress, ushort getCount)
        {
            try
            {
                var inputStruct = new ReadCoilStatusModbusProtocal.ReadCoilStatusInputStruct(belongAddress, startAddress,
                getCount);
                var outputStruct =
                    _wrapper.SendReceive(_wrapper["ReadCoilStatusModbusProtocal"], inputStruct) as
                        ReadCoilStatusModbusProtocal.ReadCoilStatusOutputStruct;
                return outputStruct.CoilStatus;
            }
            catch (Exception)
            {
                return null;
            }           
        }

        public override bool SetCoils(byte belongAddress, string startAddress, bool[] setContents)
        {
            try
            {
                var inputStruct = new WriteMultiCoilModbusProtocal.WriteMultiCoilInputStruct(belongAddress, startAddress,
                setContents);
                var outputStruct =
                    _wrapper.SendReceive(_wrapper["WriteMultiCoilModbusProtocal"], inputStruct) as
                        WriteMultiCoilModbusProtocal.WriteMultiCoilOutputStruct;
                if (outputStruct.WriteCount != setContents.Length) return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }    
        }

        public override ushort[] GetRegisters(byte belongAddress, string startAddress, ushort getCount)
        {
            try
            {
                var inputStruct = new ReadHoldRegisterModbusProtocal.ReadHoldRegisterInputStruct(belongAddress, startAddress, getCount);
                var outputStruct =
                    _wrapper.SendReceive(_wrapper["ReadHoldRegisterModbusProtocal"], inputStruct) as
                        ReadHoldRegisterModbusProtocal.ReadHoldRegisterOutputStruct;
                return outputStruct.HoldRegisterStatus;
            }
            catch
            {
                return null;
            }    
        }

        public override bool SetRegisters(byte belongAddress, string startAddress, object[] setContents)
        {
            try
            {
                var inputStruct = new WriteMultiRegisterModbusProtocal.WriteMultiRegisterInputStruct(belongAddress,
                    startAddress, setContents);
                var outputStruct =
                    _wrapper.SendReceive(_wrapper["WriteMultiRegisterModbusProtocal"], inputStruct) as
                        WriteMultiRegisterModbusProtocal.WriteMultiRegisterOutputStruct;
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
