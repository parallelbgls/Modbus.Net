using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

public enum ModbusProtocalFunctionCode
{
    
}

internal enum ModbusProtocalTimeFunctionCode
{
    GetSystemTime = 3,
    SetSystemTime = 16,
};

public enum ModbusProtocalReadDataFunctionCode
{
    ReadCoilStatus = 1,
    ReadInputStatus = 2,
    ReadHoldRegister = 3,
    ReadInputRegister = 4,
}

public enum ModbusProtocalWriteDataFunctionCode
{
    WriteMultiCoil = 15,
    WriteMultiRegister = 16,
}
namespace ModBus.Net
{
    public abstract class ModbusProtocal : BaseProtocal
    {

    }

    public class ReadDataInputStruct : InputStruct
    {
        public ReadDataInputStruct(byte belongAddress, ModbusProtocalReadDataFunctionCode functionCode, string startAddress, ushort getCount)
        {
            BelongAddress = belongAddress;
            FunctionCode = (byte)functionCode;
            StartAddress = AddressTranslator.Instance.AddressTranslate(startAddress);
            GetCount = getCount;
        }

        public byte BelongAddress { get; private set; }

        public byte FunctionCode { get; private set; }

        public ushort StartAddress { get; private set; }

        public ushort GetCount { get; private set; }
    }

    public class ReadDataOutputStruct : OutputStruct
    {
        public ReadDataOutputStruct(byte belongAddress, byte functionCode,
            int dataCount, byte[] dataValue)
        {
            BelongAddress = belongAddress;
            FunctionCode = functionCode;
            DataCount = dataCount;
            DataValue = dataValue.Clone() as byte[];
        }

        public byte BelongAddress { get; private set; }

        public byte FunctionCode { get; private set; }

        public int DataCount { get; private set; }

        public byte[] DataValue { get; private set; }
    }

    public class ReadDataModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadDataInputStruct)message;
            return Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte dataCount = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte[] dataValue = new byte[dataCount];
            Array.Copy(messageBytes, 3, dataValue, 0, dataCount);
            return new ReadDataOutputStruct(belongAddress, functionCode, dataCount, dataValue);
        }
    }

    public class WriteDataInputStruct : InputStruct
    {
        public WriteDataInputStruct(byte belongAddress, ModbusProtocalWriteDataFunctionCode functionCode, string startAddress, object[] writeValue)
        {
            BelongAddress = belongAddress;
            FunctionCode = (byte)functionCode;
            StartAddress = AddressTranslator.Instance.AddressTranslate(startAddress);
            WriteCount = (ushort)writeValue.Length;
            WriteByteCount = 0;
            WriteValue = writeValue.Clone() as object[];
        }

        public byte BelongAddress { get; private set; }

        public byte FunctionCode { get; private set; }

        public ushort StartAddress { get; private set; }

        public ushort WriteCount { get; private set; }

        public byte WriteByteCount { get; private set; }

        public object[] WriteValue { get; private set; }
    }

    public class WriteDataOutputStruct : OutputStruct
    {
        public WriteDataOutputStruct(byte belongAddress, byte functionCode,
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

    /// <summary>
    /// 写多个寄存器状态
    /// </summary>
    public class WriteDataModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteDataInputStruct)message;
            byte[] formattingBytes = Format(r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, r_message.WriteValue);
            formattingBytes[6] = (byte)(formattingBytes.Length - 7);
            return formattingBytes;
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeCount = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteDataOutputStruct(belongAddress, functionCode, startAddress,
                writeCount);
        }
    }

    public class GetSystemTimeInputStruct : InputStruct
    {
        public GetSystemTimeInputStruct(byte belongAddress)
        {
            BelongAddress = belongAddress;
            FunctionCode = (byte)ModbusProtocalTimeFunctionCode.GetSystemTime;
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
    }

    public class SetSystemTimeInputStruct : InputStruct
    {
        public SetSystemTimeInputStruct(byte belongAddress, DateTime time)
        {
            BelongAddress = belongAddress;
            FunctionCode = (byte)ModbusProtocalTimeFunctionCode.SetSystemTime;
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