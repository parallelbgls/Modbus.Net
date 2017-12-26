using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     变量功能码
    /// </summary>
    internal enum ModbusProtocolVariableFunctionCode : byte
    {
        /// <summary>
        ///     读变量
        /// </summary>
        ReadVariable = 20,

        /// <summary>
        ///     写变量
        /// </summary>
        WriteVariable = 21
    }

    /// <summary>
    ///     跟时间有关的功能码
    /// </summary>
    public enum ModbusProtocolTimeFunctionCode : byte
    {
        /// <summary>
        ///     读时间
        /// </summary>
        GetSystemTime = 3,

        /// <summary>
        ///     写时间
        /// </summary>
        SetSystemTime = 16
    }

    /// <summary>
    ///     跟读数据有关的功能码
    /// </summary>
    public enum ModbusProtocolReadDataFunctionCode : byte
    {
        /// <summary>
        ///     读线圈
        /// </summary>
        ReadCoilStatus = 1,

        /// <summary>
        ///     读输入线圈
        /// </summary>
        ReadInputStatus = 2,

        /// <summary>
        ///     读保持寄存器
        /// </summary>
        ReadHoldRegister = 3,

        /// <summary>
        ///     读输入寄存器
        /// </summary>
        ReadInputRegister = 4
    }

    /// <summary>
    ///     跟写数据有关的功能码
    /// </summary>
    internal enum ModbusProtocolWriteDataFunctionCode : byte
    {
        /// <summary>
        ///     写单个线圈
        /// </summary>
        WriteSingleCoil = 5,

        /// <summary>
        ///     写单个寄存器
        /// </summary>
        WriteSingleRegister = 6,

        /// <summary>
        ///     写多个线圈
        /// </summary>
        WriteMultiCoil = 15,

        /// <summary>
        ///     写多个寄存器
        /// </summary>
        WriteMultiRegister = 16
    }

    /// <summary>
    ///     Modbus协议
    /// </summary>
    public abstract class ModbusProtocol : BaseProtocol
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        protected ModbusProtocol(byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress, Endian.BigEndianLsb)
        {
        }

        /// <summary>
        ///     连接
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override async Task<bool> ConnectAsync()
        {
            return await ProtocolLinker.ConnectAsync();
        }
    }

    #region 读PLC数据

    /// <summary>
    ///     读数据输入
    /// </summary>
    public class ReadDataModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getCount">读取个数</param>
        /// <param name="addressTranslator">地址翻译器</param>
        public ReadDataModbusInputStruct(byte slaveAddress, string startAddress, ushort getCount,
            AddressTranslator addressTranslator)
        {
            SlaveAddress = slaveAddress;
            var translateAddress = addressTranslator.AddressTranslate(startAddress, true);
            FunctionCode = (byte) translateAddress.Area;
            StartAddress = (ushort) translateAddress.Address;
            GetCount =
                (ushort) Math.Ceiling(getCount / addressTranslator.GetAreaByteLength(translateAddress.AreaString));
        }

        /// <summary>
        ///     从站地址
        /// </summary>
        public byte SlaveAddress { get; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; }

        /// <summary>
        ///     开始地址
        /// </summary>
        public ushort StartAddress { get; }

        /// <summary>
        ///     获取个数
        /// </summary>
        public ushort GetCount { get; }
    }

    /// <summary>
    ///     读数据输出
    /// </summary>
    public class ReadDataModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="dataCount">数据个数</param>
        /// <param name="dataValue">读取的数据值</param>
        public ReadDataModbusOutputStruct(byte slaveAddress, byte functionCode,
            int dataCount, byte[] dataValue)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            DataCount = dataCount;
            DataValue = dataValue.Clone() as byte[];
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; private set; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; private set; }

        /// <summary>
        ///     数据个数
        /// </summary>
        public int DataCount { get; private set; }

        /// <summary>
        ///     数据值
        /// </summary>
        public byte[] DataValue { get; private set; }
    }

    /// <summary>
    ///     读数据协议
    /// </summary>
    public class ReadDataModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">读取参数</param>
        /// <returns>读取数据的协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ReadDataModbusInputStruct) message;
            return Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        /// <summary>
        ///     反格式化
        /// </summary>
        /// <param name="messageBytes">设备返回的信息</param>
        /// <param name="pos">当前反格式化的位置</param>
        /// <returns>反格式化的信息</returns>
        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            var slaveAddress = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref pos);
            var functionCode = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref pos);
            var dataCount = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref pos);
            var dataValue = new byte[dataCount];
            Array.Copy(messageBytes, 3, dataValue, 0, dataCount);
            return new ReadDataModbusOutputStruct(slaveAddress, functionCode, dataCount, dataValue);
        }
    }

    #endregion

    #region 写PLC数据

    /// <summary>
    ///     写数据输入
    /// </summary>
    public class WriteDataModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="writeValue">写入的数据</param>
        /// <param name="addressTranslator">地址翻译器</param>
        /// <param name="endian">端格式</param>
        public WriteDataModbusInputStruct(byte slaveAddress, string startAddress, object[] writeValue,
            AddressTranslator addressTranslator, Endian endian)
        {
            SlaveAddress = slaveAddress;
            var translateAddress = addressTranslator.AddressTranslate(startAddress, false);
            FunctionCode = (byte) translateAddress.Area;
            StartAddress = (ushort) translateAddress.Address;
            var writeByteValue = ValueHelper.GetInstance(endian).ObjectArrayToByteArray(writeValue);
            WriteCount =
                (ushort) (writeByteValue.Length / addressTranslator.GetAreaByteLength(translateAddress.AreaString));
            WriteByteCount = (byte) writeByteValue.Length;
            WriteValue = writeByteValue;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; }

        /// <summary>
        ///     开始地址
        /// </summary>
        public ushort StartAddress { get; }

        /// <summary>
        ///     写入个数
        /// </summary>
        public ushort WriteCount { get; }

        /// <summary>
        ///     写入字节个数
        /// </summary>
        public byte WriteByteCount { get; }

        /// <summary>
        ///     写入的数据
        /// </summary>
        public byte[] WriteValue { get; }
    }

    /// <summary>
    ///     写数据输出
    /// </summary>
    public class WriteDataModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="writeCount">写入个数</param>
        public WriteDataModbusOutputStruct(byte slaveAddress, byte functionCode,
            ushort startAddress, ushort writeCount)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            StartAddress = startAddress;
            WriteCount = writeCount;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; private set; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; private set; }

        /// <summary>
        ///     开始地址
        /// </summary>
        public ushort StartAddress { get; private set; }

        /// <summary>
        ///     写入个数
        /// </summary>
        public ushort WriteCount { get; private set; }
    }

    /// <summary>
    ///     写多个寄存器协议
    /// </summary>
    public class WriteDataModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (WriteDataModbusInputStruct) message;
            var dataValue = Format(r_message.WriteValue);
            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, dataValue);
            return formattingBytes;
        }

        /// <summary>
        ///     反格式化
        /// </summary>
        /// <param name="messageBytes">设备返回的信息</param>
        /// <param name="flag">当前反格式化的位置</param>
        /// <returns>反格式化的信息</returns>
        public override IOutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            var slaveAddress = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var functionCode = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var startAddress = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var writeCount = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            return new WriteDataModbusOutputStruct(slaveAddress, functionCode, startAddress,
                writeCount);
        }
    }

    /// <summary>
    ///     写数据输入
    /// </summary>
    public class WriteSingleDataModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="writeValue">写入的数据</param>
        /// <param name="addressTranslator">地址翻译器</param>
        /// <param name="endian">端格式</param>
        public WriteSingleDataModbusInputStruct(byte slaveAddress, string startAddress, object writeValue,
            ModbusTranslatorBase addressTranslator, Endian endian)
        {
            SlaveAddress = slaveAddress;
            var translateAddress = addressTranslator.AddressTranslate(startAddress, false, true);
            FunctionCode = (byte) translateAddress.Area;
            StartAddress = (ushort) translateAddress.Address;
            var writeByteValue =
                FunctionCode == (byte) ModbusProtocolWriteDataFunctionCode.WriteSingleCoil
                    ? ((bool) writeValue
                        ? new byte[] {0xFF, 0x00}
                        : new byte[] {0x00, 0x00})
                    : ValueHelper.GetInstance(endian).GetBytes(ushort.Parse(writeValue.ToString()));
            WriteValue = writeByteValue;
        }


        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; }

        /// <summary>
        ///     开始地址
        /// </summary>
        public ushort StartAddress { get; }

        /// <summary>
        ///     写入的数据
        /// </summary>
        public byte[] WriteValue { get; }
    }

    /// <summary>
    ///     写数据输出
    /// </summary>
    public class WriteSingleDataModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="writeValue">写入的数据</param>
        public WriteSingleDataModbusOutputStruct(byte slaveAddress, byte functionCode,
            ushort startAddress, object writeValue)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            StartAddress = startAddress;
            WriteValue = writeValue;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; private set; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; private set; }

        /// <summary>
        ///     开始地址
        /// </summary>
        public ushort StartAddress { get; private set; }

        /// <summary>
        ///     写入的数据
        /// </summary>
        public object WriteValue { get; private set; }
    }

    /// <summary>
    ///     写多个寄存器协议
    /// </summary>
    public class WriteSingleDataModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (WriteSingleDataModbusInputStruct) message;
            var dataValue = Format(r_message.WriteValue);
            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.StartAddress, dataValue);
            return formattingBytes;
        }

        /// <summary>
        ///     反格式化
        /// </summary>
        /// <param name="messageBytes">设备返回的信息</param>
        /// <param name="flag">当前反格式化的位置</param>
        /// <returns>反格式化的信息</returns>
        public override IOutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            var slaveAddress = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var functionCode = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var startAddress = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var writeValue = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var returnValue = functionCode == (byte)ModbusProtocolWriteDataFunctionCode.WriteSingleCoil
                ? (object)(writeValue == 0xFF00) : writeValue;
            return new WriteSingleDataModbusOutputStruct(slaveAddress, functionCode, startAddress,
                returnValue);
        }
    }

    #endregion

    #region 读PLC时间

    /// <summary>
    ///     读时间输入
    /// </summary>
    public class GetSystemTimeModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        public GetSystemTimeModbusInputStruct(byte slaveAddress)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte) ModbusProtocolTimeFunctionCode.GetSystemTime;
            StartAddress = 30000;
            GetCount = 5;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; }

        /// <summary>
        ///     开始地址
        /// </summary>
        public ushort StartAddress { get; }

        /// <summary>
        ///     获取个数
        /// </summary>
        public ushort GetCount { get; }
    }

    /// <summary>
    ///     读时间输出
    /// </summary>
    public class GetSystemTimeModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="writeByteCount">写入个数</param>
        /// <param name="year">年</param>
        /// <param name="day">日</param>
        /// <param name="month">月</param>
        /// <param name="hour">时</param>
        /// <param name="second">秒</param>
        /// <param name="minute">分</param>
        /// <param name="millisecond">毫秒</param>
        public GetSystemTimeModbusOutputStruct(byte slaveAddress, byte functionCode,
            byte writeByteCount, ushort year, byte day, byte month, ushort hour, byte second, byte minute,
            ushort millisecond)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            WriteByteCount = writeByteCount;
            Time = new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; private set; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; private set; }

        /// <summary>
        ///     写入个数
        /// </summary>
        public byte WriteByteCount { get; private set; }

        /// <summary>
        ///     时间
        /// </summary>
        public DateTime Time { get; private set; }
    }

    /// <summary>
    ///     读系统时间协议
    /// </summary>
    public class GetSystemTimeModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写系统时间参数</param>
        /// <returns>写系统时间的核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (GetSystemTimeModbusInputStruct) message;
            return Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        /// <summary>
        ///     反格式化
        /// </summary>
        /// <param name="messageBytes">获取的信息</param>
        /// <param name="flag">当前反格式化的位置</param>
        /// <returns>反格式化的信息</returns>
        public override IOutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            var slaveAddress = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var functionCode = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var writeByteCount = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var year = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var day = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var month = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var hour = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var second = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var minute = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var millisecond = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            return new GetSystemTimeModbusOutputStruct(slaveAddress, functionCode, writeByteCount, year, day,
                month, hour, second, minute, millisecond);
        }
    }

    #endregion

    #region 写PLC时间

    /// <summary>
    ///     写时间输入
    /// </summary>
    public class SetSystemTimeModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="time">时间</param>
        public SetSystemTimeModbusInputStruct(byte slaveAddress, DateTime time)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte) ModbusProtocolTimeFunctionCode.SetSystemTime;
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

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; }

        /// <summary>
        ///     开始地址
        /// </summary>
        public ushort StartAddress { get; }

        /// <summary>
        ///     写入个数
        /// </summary>
        public ushort WriteCount { get; }

        /// <summary>
        ///     写入字节个数
        /// </summary>
        public byte WriteByteCount { get; }

        /// <summary>
        ///     年
        /// </summary>
        public ushort Year { get; }

        /// <summary>
        ///     日
        /// </summary>
        public byte Day { get; }

        /// <summary>
        ///     月
        /// </summary>
        public byte Month { get; }

        /// <summary>
        ///     时
        /// </summary>
        public ushort Hour { get; }

        /// <summary>
        ///     秒
        /// </summary>
        public byte Second { get; }

        /// <summary>
        ///     分
        /// </summary>
        public byte Minute { get; }

        /// <summary>
        ///     毫秒
        /// </summary>
        public ushort Millisecond { get; }
    }

    /// <summary>
    ///     写时间输出
    /// </summary>
    public class SetSystemTimeModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="writeCount">写入个数</param>
        public SetSystemTimeModbusOutputStruct(byte slaveAddress, byte functionCode,
            ushort startAddress, ushort writeCount)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            StartAddress = startAddress;
            WriteCount = writeCount;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; private set; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; private set; }

        /// <summary>
        ///     开始地址
        /// </summary>
        public ushort StartAddress { get; private set; }

        /// <summary>
        ///     写入个数
        /// </summary>
        public ushort WriteCount { get; private set; }
    }

    /// <summary>
    ///     写系统时间协议
    /// </summary>
    public class SetSystemTimeModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写系统时间的参数</param>
        /// <returns>写系统时间的核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (SetSystemTimeModbusInputStruct) message;
            return Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, r_message.Year,
                r_message.Day,
                r_message.Month, r_message.Hour, r_message.Second, r_message.Minute, r_message.Millisecond);
        }

        /// <summary>
        ///     反格式化
        /// </summary>
        /// <param name="messageBytes">获取的信息</param>
        /// <param name="flag">当前反格式化的位置</param>
        /// <returns>反格式化的信息</returns>
        public override IOutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            var slaveAddress = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var functionCode = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var startAddress = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var writeCount = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            return new SetSystemTimeModbusOutputStruct(slaveAddress, functionCode, startAddress, writeCount);
        }
    }

    #endregion

    /// <summary>
    ///     Modbus协议错误表
    /// </summary>
    public class ModbusProtocolErrorException : ProtocolErrorException
    {
        private static readonly Dictionary<int, string> ProtocolErrorDictionary = new Dictionary<int, string>
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

        /// <summary>
        ///     Modbus错误
        /// </summary>
        /// <param name="messageNumber">Modbus错误号</param>
        public ModbusProtocolErrorException(int messageNumber)
            : base(ProtocolErrorDictionary[messageNumber])
        {
            ErrorMessageNumber = messageNumber;
        }

        /// <summary>
        ///     Modbus错误号
        /// </summary>
        public int ErrorMessageNumber { get; private set; }
    }
}