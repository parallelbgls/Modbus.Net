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
    public class ModbusUtility
    {
        private BaseProtocal _wrapper;

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
                        _wrapper = new ModbusRtuProtocal();
                        break;
                    }
                    case ModbusType.Tcp:
                    {
                        _wrapper = new ModbusTcpProtocal();
                        break;
                    }
                }
            }
        }

        private static ModbusUtility _modbusUtility;
        private ModbusUtility()
        {
            ModbusType = ModbusType.Rtu;
        }

        public static ModbusUtility GetInstance()
        {
            return _modbusUtility ?? (_modbusUtility = new ModbusUtility());
        }

        public ushort[] ReadHoldRegister(byte belongAddress, string startAddress, ushort getCount)
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

        public bool WriteMultiRegister(byte belongAddress, string startAddress, object[] writeValue)
        {
            try
            {
                var inputStruct = new WriteMultiRegisterModbusProtocal.WriteMultiRegisterInputStruct(belongAddress,
                    startAddress, writeValue);
                var outputStruct =
                    _wrapper.SendReceive(_wrapper["WriteMultiRegisterModbusProtocal"], inputStruct) as
                        WriteMultiRegisterModbusProtocal.WriteMultiRegisterOutputStruct;
                if (outputStruct.WriteCount != writeValue.Length) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
