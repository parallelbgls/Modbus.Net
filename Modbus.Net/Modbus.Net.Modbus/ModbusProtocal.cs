using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    internal enum ModbusProtocalVariableFunctionCode : byte
    {
        ReadVariable = 20,
        WriteVariable = 21
    }

    /// <summary>
    ///     跟时间有关的功能码
    /// </summary>
    public enum ModbusProtocalTimeFunctionCode : byte
    {
        GetSystemTime = 3,
        SetSystemTime = 16
    }

    /// <summary>
    ///     跟读数据有关的功能码
    /// </summary>
    public enum ModbusProtocalReadDataFunctionCode : byte
    {
        ReadCoilStatus = 1,
        ReadInputStatus = 2,
        ReadHoldRegister = 3,
        ReadInputRegister = 4
    }

    /// <summary>
    ///     跟写数据有关的功能码
    /// </summary>
    internal enum ModbusProtocalWriteDataFunctionCode : byte
    {
        WriteMultiCoil = 15,
        WriteMultiRegister = 16
    }

    public abstract class ModbusProtocal : BaseProtocal
    {
        protected ModbusProtocal(byte slaveAddress, byte masterAddress) : base(slaveAddress, masterAddress)
        {
        }

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
        public ReadDataModbusInputStruct(byte slaveAddress, string startAddress, ushort getCount,
            AddressTranslator addressTranslator)
        {
            SlaveAddress = slaveAddress;
            var translateAddress = addressTranslator.AddressTranslate(startAddress, true);
            FunctionCode = (byte) translateAddress.Area;
            StartAddress = (ushort) translateAddress.Address;
            GetCount = (ushort) Math.Ceiling(getCount/addressTranslator.GetAreaByteLength(translateAddress.AreaString));
        }

        public byte SlaveAddress { get; }

        public byte FunctionCode { get; }

        public ushort StartAddress { get; }

        public ushort GetCount { get; }
    }

    public class ReadDataModbusOutputStruct : OutputStruct
    {
        public ReadDataModbusOutputStruct(byte slaveAddress, byte functionCode,
            int dataCount, byte[] dataValue)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            DataCount = dataCount;
            DataValue = dataValue.Clone() as byte[];
        }

        public byte SlaveAddress { get; private set; }

        public byte FunctionCode { get; private set; }

        public int DataCount { get; private set; }

        public byte[] DataValue { get; private set; }
    }

    public class ReadDataModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadDataModbusInputStruct) message;
            return Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            var slaveAddress = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            var functionCode = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            var dataCount = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            var dataValue = new byte[dataCount];
            Array.Copy(messageBytes, 3, dataValue, 0, dataCount);
            if (functionCode == 1 || functionCode == 2)
            {
                for (var i = 0; i < dataValue.Length; i++)
                {
                    dataValue[i] = dataValue[i];
                }
            }
            return new ReadDataModbusOutputStruct(slaveAddress, functionCode, dataCount, dataValue);
        }
    }

    #endregion

    #region 写PLC数据

    public class WriteDataModbusInputStruct : InputStruct
    {
        public WriteDataModbusInputStruct(byte slaveAddress, string startAddress, object[] writeValue,
            AddressTranslator addressTranslator)
        {
            SlaveAddress = slaveAddress;
            var translateAddress = addressTranslator.AddressTranslate(startAddress, false);
            FunctionCode = (byte) translateAddress.Area;
            StartAddress = (ushort) translateAddress.Address;
            var writeByteValue = BigEndianValueHelper.Instance.ObjectArrayToByteArray(writeValue);
            WriteCount =
                (ushort) (writeByteValue.Length/addressTranslator.GetAreaByteLength(translateAddress.AreaString));
            WriteByteCount = (byte) writeByteValue.Length;
            WriteValue = writeByteValue;
        }

        public byte SlaveAddress { get; }

        public byte FunctionCode { get; }

        public ushort StartAddress { get; }

        public ushort WriteCount { get; }

        public byte WriteByteCount { get; }

        public byte[] WriteValue { get; }
    }

    public class WriteDataModbusOutputStruct : OutputStruct
    {
        public WriteDataModbusOutputStruct(byte slaveAddress, byte functionCode,
            ushort startAddress, ushort writeCount)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            StartAddress = startAddress;
            WriteCount = writeCount;
        }

        public byte SlaveAddress { get; private set; }

        public byte FunctionCode { get; private set; }

        public ushort StartAddress { get; private set; }

        public ushort WriteCount { get; private set; }
    }

    /// <summary>
    ///     写多个寄存器状态
    /// </summary>
    public class WriteDataModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteDataModbusInputStruct) message;
            var functionCode = r_message.FunctionCode;
            var dataValue = Format(r_message.WriteValue);
            if (functionCode == 5 || functionCode == 15)
            {
                for (var i = 0; i < dataValue.Length; i++)
                {
                    dataValue[i] = dataValue[i];
                }
            }
            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, dataValue);
            return formattingBytes;
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            var slaveAddress = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var functionCode = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var startAddress = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref flag);
            var writeCount = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteDataModbusOutputStruct(slaveAddress, functionCode, startAddress,
                writeCount);
        }
    }

    #endregion

    #region 读PLC时间

    public class GetSystemTimeModbusInputStruct : InputStruct
    {
        public GetSystemTimeModbusInputStruct(byte slaveAddress)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte) ModbusProtocalTimeFunctionCode.GetSystemTime;
            StartAddress = 30000;
            GetCount = 5;
        }

        public byte SlaveAddress { get; }

        public byte FunctionCode { get; }

        public ushort StartAddress { get; }

        public ushort GetCount { get; }
    }

    public class GetSystemTimeModbusOutputStruct : OutputStruct
    {
        public GetSystemTimeModbusOutputStruct(byte slaveAddress, byte functionCode,
            byte writeByteCount, ushort year, byte day, byte month, ushort hour, byte second, byte minute,
            ushort millisecond)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            WriteByteCount = writeByteCount;
            Time = new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        public byte SlaveAddress { get; private set; }

        public byte FunctionCode { get; private set; }

        public byte WriteByteCount { get; private set; }

        public DateTime Time { get; private set; }
    }

    /// <summary>
    ///     读系统时间
    /// </summary>
    public class GetSystemTimeModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (GetSystemTimeModbusInputStruct) message;
            return Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            var slaveAddress = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var functionCode = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var writeByteCount = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var year = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref flag);
            var day = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var month = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var hour = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref flag);
            var second = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var minute = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var millisecond = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new GetSystemTimeModbusOutputStruct(slaveAddress, functionCode, writeByteCount, year, day,
                month, hour, second, minute, millisecond);
        }
    }

    #endregion

    #region 写PLC时间

    public class SetSystemTimeModbusInputStruct : InputStruct
    {
        public SetSystemTimeModbusInputStruct(byte slaveAddress, DateTime time)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte) ModbusProtocalTimeFunctionCode.SetSystemTime;
            StartAddress = 30000;
            WriteCount = 5;
            WriteByteCount = 10;
            Year = (ushort) time.Year;
            Day = (byte) time.Day;
            Month = (byte) time.Month;
            Hour = (ushort) time.Hour;
            Second = (byte) time.Second;
            Minute = (byte) time.Minute;
            Millisecond = (ushort) time.Millisecond;
        }

        public byte SlaveAddress { get; }

        public byte FunctionCode { get; }

        public ushort StartAddress { get; }

        public ushort WriteCount { get; }

        public byte WriteByteCount { get; }

        public ushort Year { get; }

        public byte Day { get; }

        public byte Month { get; }

        public ushort Hour { get; }

        public byte Second { get; }

        public byte Minute { get; }

        public ushort Millisecond { get; }
    }

    public class SetSystemTimeModbusOutputStruct : OutputStruct
    {
        public SetSystemTimeModbusOutputStruct(byte slaveAddress, byte functionCode,
            ushort startAddress, ushort writeCount)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            StartAddress = startAddress;
            WriteCount = writeCount;
        }

        public byte SlaveAddress { get; private set; }

        public byte FunctionCode { get; private set; }

        public ushort StartAddress { get; private set; }

        public ushort WriteCount { get; private set; }
    }

    /// <summary>
    ///     写系统时间
    /// </summary>
    public class SetSystemTimeModbusProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (SetSystemTimeModbusInputStruct) message;
            return Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, r_message.Year,
                r_message.Day,
                r_message.Month, r_message.Hour, r_message.Second, r_message.Minute, r_message.Millisecond);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            var slaveAddress = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var functionCode = BigEndianValueHelper.Instance.GetByte(messageBytes, ref flag);
            var startAddress = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref flag);
            var writeCount = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new SetSystemTimeModbusOutputStruct(slaveAddress, functionCode, startAddress, writeCount);
        }
    }

    #endregion

    /// <summary>
    ///     Modbus协议错误表
    /// </summary>
    public class ModbusProtocalErrorException : ProtocalErrorException
    {
        private static readonly Dictionary<int, string> ProtocalErrorDictionary = new Dictionary<int, string>
        {
            {1, "ILLEGAL_FUNCTION"},
            {2, "ILLEGAL_DATA_ACCESS"},
            {3, "ILLEGAL_DATA_VALUE"},
            {4, "SLAVE_DEVICE_FAILURE"},
            {5, "ACKNOWLWDGE"},
            {6, "SLAVE_DEVICE_BUSY"},
            {500, "TCP_ILLEGAL_LENGTH"},
            {501, "RTU_ILLEGAL_CRC"}
        };

        public ModbusProtocalErrorException(int messageNumber)
            : base(ProtocalErrorDictionary[messageNumber])
        {
            ErrorMessageNumber = messageNumber;
        }

        public int ErrorMessageNumber { get; private set; }
    }
}