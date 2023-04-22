using System;
using ProtocolUnit = Modbus.Net.ProtocolUnit<byte[], byte[]>;

namespace Modbus.Net.Modbus.SelfDefinedSample
{
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
        public GetSystemTimeModbusInputStruct(byte slaveAddress, ushort startAddress)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusProtocolTimeFunctionCode.GetSystemTime;
            StartAddress = startAddress;
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
            var r_message = (GetSystemTimeModbusInputStruct)message;
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
        public SetSystemTimeModbusInputStruct(byte slaveAddress, ushort startAddress, DateTime time)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusProtocolTimeFunctionCode.SetSystemTime;
            StartAddress = startAddress;
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
            var r_message = (SetSystemTimeModbusInputStruct)message;
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
}