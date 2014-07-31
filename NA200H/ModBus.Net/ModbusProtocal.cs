public enum ModbusProtocalReg
{
    ReadCoilStatus = 1,
    ReadInputStatus = 2,
    ReadHoldRegister = 3,
    ReadInputRegister = 4,
    WriteOneCoil = 5,
    WriteOneRegister = 6,
    WriteMultiCoil = 15,
    WriteMultiRegister = 16,
    GetSystemTime = 3,
    SetSystemTime = 16,
    ReadVariable = 20,
    WriteVariable = 21,
};

namespace ModBus.Net
{
    public abstract class ModbusProtocal : BaseProtocal
    {
    }
}