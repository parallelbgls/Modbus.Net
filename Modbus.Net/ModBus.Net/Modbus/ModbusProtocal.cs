using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModBus.Net.Modbus
{
    internal enum ModbusProtocalVariableFunctionCode : byte
    {
        ReadVariable = 20,
        WriteVariable = 21,
    }

    /// <summary>
    /// 跟时间有关的功能码
    /// </summary>
    public enum ModbusProtocalTimeFunctionCode : byte
    {
        GetSystemTime = 3,
        SetSystemTime = 16,
    }

    /// <summary>
    /// 跟读数据有关的功能码
    /// </summary>
    public enum ModbusProtocalReadDataFunctionCode : byte
    {
        ReadCoilStatus = 1,
        ReadInputStatus = 2,
        ReadHoldRegister = 3,
        ReadInputRegister = 4,
    }

    /// <summary>
    /// 跟写数据有关的功能码
    /// </summary>
    internal enum ModbusProtocalWriteDataFunctionCode : byte
    {
        WriteMultiCoil = 15,
        WriteMultiRegister = 16,
    }

    public abstract class ModbusProtocal : BaseProtocal
    {
        public override bool Connect()
        {
            return ProtocalLinker.Connect();
        }

        public override async Task<bool> ConnectAsync()
        {
            return await ProtocalLinker.ConnectAsync();
        }
    }

    #region 读PLC数据
    public class ReadDataModbusInputStruct : InputStruct
    {
        public ReadDataModbusInputStruct(byte belongAddress, string startAddress, ushort getCount, AddressTranslator addressTranslator)
        {
            BelongAddress = belongAddress;
            KeyValuePair<int, int> translateAddress = addressTranslator.AddressTranslate(startAddress, true);
            FunctionCode = (byte)translateAddress.Value;
            StartAddress = (ushort)translateAddress.Key;
            GetCount = getCount;
        }

        public byte BelongAddress { get; private set; }

        public byte FunctionCode { get; private set; }

        public ushort StartAddress { get; private set; }

        public ushort GetCount { get; private set; }
    }

    public class ReadDataModbusOutputStruct : OutputStruct
    {
        public ReadDataModbusOutputStruct(byte belongAddress, byte functionCode,
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
            var r_message = (ReadDataModbusInputStruct)message;
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
            return new ReadDataModbusOutputStruct(belongAddress, functionCode, dataCount, dataValue);
        }
    }

    #endregion

    #region 写PLC数据
    public class WriteDataModbusInputStruct : InputStruct
    {
        public WriteDataModbusInputStruct(byte belongAddress, string startAddress, object[] writeValue, AddressTranslator addressTranslator)
        {
            BelongAddress = belongAddress;
            KeyValuePair<int, int> translateAddress = addressTranslator.AddressTranslate(startAddress, false);
            FunctionCode = (byte)translateAddress.Value;
            StartAddress = (ushort)translateAddress.Key;
            WriteCount = (ushort)writeValue.Length;
            WriteByteCount = 0;
            WriteValue = writeValue;
        }

        public byte BelongAddress { get; private set; }

        public byte FunctionCode { get; private set; }

        public ushort StartAddress { get; private set; }

        public ushort WriteCount { get; private set; }

        public byte WriteByteCount { get; private set; }

        public object[] WriteValue { get; private set; }
    }

    public class WriteDataModbusOutputStruct : OutputStruct
    {
        public WriteDataModbusOutputStruct(byte belongAddress, byte functionCode,
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
            var r_message = (WriteDataModbusInputStruct)message;
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
            return new WriteDataModbusOutputStruct(belongAddress, functionCode, startAddress,
                writeCount);
        }
    }

    #endregion

    #region 读PLC时间
    public class GetSystemTimeModbusInputStruct : InputStruct
    {
        public GetSystemTimeModbusInputStruct(byte belongAddress)
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

    public class GetSystemTimeModbusOutputStruct : OutputStruct
    {
        public GetSystemTimeModbusOutputStruct(byte belongAddress, byte functionCode,
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
            var r_message = (GetSystemTimeModbusInputStruct)message;
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
            return new GetSystemTimeModbusOutputStruct(belongAddress, functionCode, writeByteCount, year, day,
                month, hour, second, minute, millisecond);
        }
    }

    #endregion

    #region 写PLC时间
    public class SetSystemTimeModbusInputStruct : InputStruct
    {
        public SetSystemTimeModbusInputStruct(byte belongAddress, DateTime time)
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

    public class SetSystemTimeModbusOutputStruct : OutputStruct
    {
        public SetSystemTimeModbusOutputStruct(byte belongAddress, byte functionCode,
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
            var r_message = (SetSystemTimeModbusInputStruct)message;
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
            return new SetSystemTimeModbusOutputStruct(belongAddress, functionCode, startAddress, writeCount);
        }
    }
    #endregion

    /// <summary>
    /// Modbus协议错误表
    /// </summary>
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