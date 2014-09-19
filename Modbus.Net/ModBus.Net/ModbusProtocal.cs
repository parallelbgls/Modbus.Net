using System;
using System.Collections.Generic;

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

    /// <summary>
    /// 读线圈状态
    /// </summary>
    public class ReadCoilStatusModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadCoilStatusInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte coilCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            var coilStatusArr = new bool[coilCount * 8];
            for (int i = 0; i < coilCount; i++)
            {
                byte coilStatusGet = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                for (int j = 0; j < 8; j++)
                {
                    if (coilStatusGet % 2 == 0) coilStatusArr[8 * i + j] = false;
                    else coilStatusArr[8 * i + j] = true;
                    coilStatusGet /= 2;
                }
            }
            return new ReadCoilStatusOutputStruct(belongAddress, functionCode, coilCount * 8, coilStatusArr);
        }

        public class ReadCoilStatusInputStruct : InputStruct
        {
            public ReadCoilStatusInputStruct(byte belongAddress, string startAddress, ushort getCount)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.ReadCoilStatus;
                StartAddress = _addressTranslator.AddressTranslate(startAddress);
                GetCount = getCount;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class ReadCoilStatusOutputStruct : OutputStruct
        {
            public ReadCoilStatusOutputStruct(byte belongAddress, byte functionCode,
                int coilCount, bool[] coilStatus)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                CoilCount = coilCount;
                CoilStatus = coilStatus.Clone() as bool[];
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public int CoilCount { get; private set; }

            public bool[] CoilStatus { get; private set; }
        }
    }

    /// <summary>
    /// 读输入状态协
    /// </summary>
    public class ReadInputStatusModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadInputStatusInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte inputCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            var inputStatusArr = new bool[inputCount * 8];
            for (int i = 0; i < inputCount; i++)
            {
                byte inputStatusGet = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                for (int j = 0; j < 8; j++)
                {
                    if (inputStatusGet % 2 == 0) inputStatusArr[8 * i + j] = false;
                    else inputStatusArr[8 * i + j] = true;
                    inputStatusGet /= 2;
                }
            }
            return new ReadInputStatusOutputStruct(belongAddress, functionCode, inputCount * 8,
                inputStatusArr);
        }

        public class ReadInputStatusInputStruct : InputStruct
        {
            public ReadInputStatusInputStruct(byte belongAddress, string startAddress, ushort getCount)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.ReadInputStatus;
                StartAddress = _addressTranslator.AddressTranslate(startAddress);
                GetCount = getCount;
            }


            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class ReadInputStatusOutputStruct : OutputStruct
        {
            public ReadInputStatusOutputStruct(byte belongAddress, byte functionCode,
                int inputCount, bool[] inputStatus)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                InputCount = inputCount;
                InputStatus = inputStatus.Clone() as bool[];
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public int InputCount { get; private set; }

            public bool[] InputStatus { get; private set; }
        }
    }

    /// <summary>
    /// 读保持型寄存器
    /// </summary>
    public class ReadHoldRegisterModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadHoldRegisterInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte holdRegisterCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            var holdRegisterArr = new ushort[holdRegisterCount / 2];
            for (int i = 0; i < holdRegisterCount / 2; i++)
            {
                holdRegisterArr[i] = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            }
            return new ReadHoldRegisterOutputStruct(belongAddress, functionCode, holdRegisterCount / 2,
                holdRegisterArr);
        }

        public class ReadHoldRegisterInputStruct : InputStruct
        {
            public ReadHoldRegisterInputStruct(byte belongAddress, string startAddress, ushort getCount)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.ReadHoldRegister;
                StartAddress = _addressTranslator.AddressTranslate(startAddress);
                GetCount = getCount;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class ReadHoldRegisterOutputStruct : OutputStruct
        {
            public ReadHoldRegisterOutputStruct(byte belongAddress, byte functionCode,
                int holdRegisterCount, ushort[] holdRegisterStatus)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                HoldRegisterCount = holdRegisterCount;
                HoldRegisterStatus = holdRegisterStatus.Clone() as ushort[];
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public int HoldRegisterCount { get; private set; }

            public ushort[] HoldRegisterStatus { get; private set; }
        }
    }

    /// <summary>
    /// 读输入型寄存器
    /// </summary>
    public class ReadInputRegisterModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadInputRegisterInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            if (messageBytes.Length == 19 && messageBytes[15] == 0 && messageBytes[17] == 0 && messageBytes[18] == 0)
            {
                byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte eventByteCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte soeProperty = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte soeEvent = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte month = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                int year = ValueHelper.Instance.GetByte(messageBytes, ref flag) + 2002;
                byte hour = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte day = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte second = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte minute = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                ushort millisecond = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
                ushort testPoint = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
                flag += 1;
                byte testValue = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                flag += 1;
                flag += 1;
                return new ReadEventOutputStruct(belongAddress, functionCode, eventByteCount, soeProperty,
                    soeEvent,
                    new DateTime(year, month == 0 ? 1 : 0, day == 0 ? 1 : 0, hour, minute, second, millisecond),
                    testPoint, testValue);
            }
            else
            {
                byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte inputRegisterCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                var holdRegisterArr = new ushort[inputRegisterCount / 2];
                for (int i = 0; i < inputRegisterCount / 2; i++)
                {
                    holdRegisterArr[i] = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
                }
                return new ReadInputRegisterOutputStruct(belongAddress, functionCode, inputRegisterCount / 2,
                    holdRegisterArr);
            }
        }

        public class ReadEventOutputStruct : OutputStruct
        {
            public ReadEventOutputStruct(byte belongAddress, byte functionCode,
                byte eventByteCount, byte soeProperty, byte soeEvent, DateTime time, ushort testPoint,
                byte testValue)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                EventByteCount = eventByteCount;
                SoeProperty = soeProperty;
                SoeEvent = soeEvent;
                TestTime = time;
                TestPoint = testPoint;
                TestValue = testValue;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public byte EventByteCount { get; private set; }

            public byte SoeProperty { get; private set; }

            public byte SoeEvent { get; private set; }

            public DateTime TestTime { get; private set; }

            public ushort TestPoint { get; private set; }

            public byte TestValue { get; private set; }
        }

        public class ReadInputRegisterInputStruct : InputStruct
        {
            public ReadInputRegisterInputStruct(byte belongAddress, string startAddress, ushort getCount)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.ReadInputRegister;
                StartAddress = _addressTranslator.AddressTranslate(startAddress);
                GetCount = getCount;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class ReadInputRegisterOutputStruct : OutputStruct
        {
            public ReadInputRegisterOutputStruct(byte belongAddress, byte functionCode,
                int inputRegisterCount, ushort[] inputRegisterStatus)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                InputRegisterCount = inputRegisterCount;
                InputRegisterStatus = inputRegisterStatus.Clone() as ushort[];
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public int InputRegisterCount { get; private set; }

            public ushort[] InputRegisterStatus { get; private set; }
        }
    }

    /// <summary>
    /// 读单个线圈状态
    /// </summary>
    public class WriteOneCoilModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteOneCoilInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteValue ? new byte[] { 0xFF, 0x00 } : new byte[] { 0x00, 0x00 });
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeValue = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteOneCoilOutputStruct(belongAddress, functionCode, startAddress,
                writeValue != 0);
        }

        public class WriteOneCoilInputStruct : InputStruct
        {
            public WriteOneCoilInputStruct(byte belongAddress, string startAddress, bool writeValue)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.WriteOneCoil;
                StartAddress = _addressTranslator.AddressTranslate(startAddress);
                WriteValue = writeValue;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public bool WriteValue { get; private set; }
        }

        public class WriteOneCoilOutputStruct : OutputStruct
        {
            public WriteOneCoilOutputStruct(byte belongAddress, byte functionCode,
                ushort startAddress, bool writeValue)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteValue = writeValue;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public bool WriteValue { get; private set; }
        }
    }

    /// <summary>
    /// 写单个寄存器状态
    /// </summary>
    public class WriteOneRegisterModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteOneRegisterInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteValue);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeValue = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteOneRegisterOutputStruct(belongAddress, functionCode, startAddress, writeValue);
        }

        public class WriteOneRegisterInputStruct : InputStruct
        {
            public WriteOneRegisterInputStruct(byte belongAddress, string startAddress, ushort writeValue)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.WriteOneRegister;
                StartAddress = _addressTranslator.AddressTranslate(startAddress);
                WriteValue = writeValue;
            }
            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteValue { get; private set; }
        }

        public class WriteOneRegisterOutputStruct : OutputStruct
        {
            public WriteOneRegisterOutputStruct(byte belongAddress, byte functionCode,
                ushort startAddress, ushort writeValue)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteValue = writeValue;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteValue { get; private set; }
        }
    }

    /// <summary>
    /// 写多个线圈状态
    /// </summary>
    public class WriteMultiCoilModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteMultiCoilInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, r_message.WriteValue);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeCount = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteMultiCoilOutputStruct(belongAddress, functionCode, startAddress, writeCount);
        }

        public class WriteMultiCoilInputStruct : InputStruct
        {
            public WriteMultiCoilInputStruct(byte belongAddress, string startAddress, bool[] writeValue)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.WriteMultiCoil;
                StartAddress = _addressTranslator.AddressTranslate(startAddress);
                WriteCount = (ushort)writeValue.Length;
                WriteByteCount = WriteCount % 8 > 0 ? (byte)(WriteCount / 8 + 1) : (byte)(WriteCount / 8);
                WriteValue = new byte[WriteByteCount];
                for (int i = 0; i < writeValue.Length; i += 8)
                {
                    int bytenum = 0;
                    for (int j = 7; j >= 0; j--)
                    {
                        int t = i + j < writeValue.Length && writeValue[i + j] ? 1 : 0;
                        bytenum = bytenum * 2 + t;
                    }
                    WriteValue[i / 8] = (byte)bytenum;
                }
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }

            public byte WriteByteCount { get; private set; }

            public byte[] WriteValue { get; private set; }
        }

        public class WriteMultiCoilOutputStruct : OutputStruct
        {
            public WriteMultiCoilOutputStruct(byte belongAddress, byte functionCode,
                ushort startAddress, ushort writeCount)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteCount = writeCount;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }
        }
    }

    /// <summary>
    /// 写多个寄存器状态
    /// </summary>
    public class WriteMultiRegisterModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteMultiRegisterInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, r_message.WriteValue);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeCount = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteMultiRegisterOutputStruct(belongAddress, functionCode, startAddress,
                writeCount);
        }

        public class WriteMultiRegisterInputStruct : InputStruct
        {
            public WriteMultiRegisterInputStruct(byte belongAddress, string startAddress, object[] writeValue)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.WriteMultiRegister;
                StartAddress = _addressTranslator.AddressTranslate(startAddress);
                WriteCount = (ushort)writeValue.Length;
                WriteByteCount = (byte)(WriteCount * 2);
                WriteValue = writeValue.Clone() as object[];
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }

            public byte WriteByteCount { get; private set; }

            public object[] WriteValue { get; private set; }
        }

        public class WriteMultiRegisterOutputStruct : OutputStruct
        {
            public WriteMultiRegisterOutputStruct(byte belongAddress, byte functionCode,
                ushort startAddress, ushort writeCount)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteCount = writeCount;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }
        }
    }

    /// <summary>
    /// 读系统时间
    /// </summary>
    public class GetSystemTimeModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (GetSystemTimeInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte writeByteCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort year = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte day = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte month = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort hour = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte second = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte minute = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort millisecond = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new GetSystemTimeOutputStruct(belongAddress, functionCode, writeByteCount, year, day,
                month, hour, second, minute, millisecond);
        }

        public class GetSystemTimeInputStruct : InputStruct
        {
            public GetSystemTimeInputStruct(byte belongAddress)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.GetSystemTime;
                StartAddress = 30000;
                GetCount = 5;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class GetSystemTimeOutputStruct : OutputStruct
        {
            public GetSystemTimeOutputStruct(byte belongAddress, byte functionCode,
                byte writeByteCount, ushort year, byte day, byte month, ushort hour, byte second, byte minute,
                ushort millisecond)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                WriteByteCount = writeByteCount;
                Time = new DateTime(year, month, day, hour, minute, second, millisecond);
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public byte WriteByteCount { get; private set; }

            public DateTime Time { get; private set; }
        }
    }

    /// <summary>
    /// 写系统时间
    /// </summary>
    public class SetSystemTimeModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (SetSystemTimeInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, r_message.Year,
                r_message.Day,
                r_message.Month, r_message.Hour, r_message.Second, r_message.Minute, r_message.Millisecond);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeCount = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new SetSystemTimeOutputStruct(belongAddress, functionCode, startAddress, writeCount);
        }

        public class SetSystemTimeInputStruct : InputStruct
        {
            public SetSystemTimeInputStruct(byte belongAddress, DateTime time)
            {
                BelongAddress = belongAddress;
                FunctionCode = (int)ModbusProtocalReg.SetSystemTime;
                StartAddress = 30000;
                WriteCount = 5;
                WriteByteCount = 10;
                Year = (ushort)time.Year;
                Day = (byte)time.Day;
                Month = (byte)time.Month;
                Hour = (ushort)time.Hour;
                Second = (byte)time.Second;
                Minute = (byte)time.Minute;
                Millisecond = (ushort)time.Millisecond;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }

            public byte WriteByteCount { get; private set; }

            public ushort Year { get; private set; }

            public byte Day { get; private set; }

            public byte Month { get; private set; }

            public ushort Hour { get; private set; }

            public byte Second { get; private set; }

            public byte Minute { get; private set; }

            public ushort Millisecond { get; private set; }
        }

        public class SetSystemTimeOutputStruct : OutputStruct
        {
            public SetSystemTimeOutputStruct(byte belongAddress, byte functionCode,
                ushort startAddress, ushort writeCount)
            {
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteCount = writeCount;
            }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }
        }

    }

    public class ProtocalErrorException : Exception
    {
        public ProtocalErrorException(string message)
            : base(message)
        {

        }
    }

    public class ModbusProtocalErrorException : ProtocalErrorException
    {
        public int ErrorMessageNumber { get; private set; }
        private static readonly Dictionary<int, string> ProtocalErrorDictionary = new Dictionary<int, string>()
        {
            {1, "ILLEGAL_FUNCTION"},
            {2, "ILLEGAL_DATA_ACCESS"},
            {3, "ILLEGAL_DATA_VALUE"},
            {4, "SLAVE_DEVICE_FAILURE"},
            {5, "ACKNOWLWDGE"},
            {6, "SLAVE_DEVICE_BUSY"},
            {500, "TCP_ILLEGAL_LENGTH"},
            {501, "RTU_ILLEGAL_CRC"},
        };

        public ModbusProtocalErrorException(int messageNumber)
            : base(ProtocalErrorDictionary[messageNumber])
        {
            ErrorMessageNumber = messageNumber;
        }
    }
}