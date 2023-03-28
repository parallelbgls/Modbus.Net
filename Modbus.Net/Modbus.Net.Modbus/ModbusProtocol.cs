using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     跟读数据有关的功能码
    /// </summary>
    public enum ModbusProtocolFunctionCode : byte
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
        ReadInputRegister = 4,

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
        WriteMultiRegister = 16,

        /// <summary>
        ///     读文件记录
        /// </summary>
        ReadFileRecord = 20,

        /// <summary>
        ///     写文件记录
        /// </summary>
        WriteFileRecord = 21,

        /// <summary>
        ///     写寄存器掩码
        /// </summary>
        MaskWriteRegister = 22,

        /// <summary>
        ///     读写多个寄存器
        /// </summary>
        ReadWriteMultipleRegister = 23,

        /// <summary>
        ///     读队列
        /// </summary>
        ReadFIFOQueue = 24,
    }

    /// <summary>
    ///     Modbus MEI方式
    /// </summary>
    public enum ModbusMEIProtocolFunctionCode : ushort
    {
        /// <summary>
        ///     
        /// </summary>
        CANopenGeneralReferenceRequestandResponsePDU = 0x2B0D,

        /// <summary>
        ///     读设备信息
        /// </summary>
        ReadDeviceIdentification = 0x2B0E
    }

    /// <summary>
    ///     只能在串口通信中使用的Modbus方法
    ///     不能在TCP和UDP通信中使用
    /// </summary>
    public enum ModbusSerialPortOnlyFunctionCode : byte
    {
        /// <summary>
        ///     读错误状态
        /// </summary>
        ReadExceptionStatus = 7,

        /// <summary>
        ///     诊断
        /// </summary>
        Diagnostics = 8,

        /// <summary>
        ///     读通讯事件计数器
        /// </summary>
        GetCommEventCounter = 11,

        /// <summary>
        ///     读日志
        /// </summary>
        GetCommEventLog = 12,

        /// <summary>
        ///     读从站ID
        /// </summary>
        ReportSlaveID = 17
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
            FunctionCode = (byte)translateAddress.Area;
            StartAddress = (ushort)translateAddress.Address;
            GetCount =
                (ushort)Math.Ceiling(getCount / addressTranslator.GetAreaByteLength(translateAddress.AreaString));
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
            var r_message = (ReadDataModbusInputStruct)message;
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
            FunctionCode = (byte)translateAddress.Area;
            StartAddress = (ushort)translateAddress.Address;
            var writeByteValue = ValueHelper.GetInstance(endian).ObjectArrayToByteArray(writeValue);
            WriteCount =
                (ushort)(writeByteValue.Length / addressTranslator.GetAreaByteLength(translateAddress.AreaString));
            WriteByteCount = (byte)writeByteValue.Length;
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
            var r_message = (WriteDataModbusInputStruct)message;
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
    public class WriteSingleCoilModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="writeValue">写入的数据</param>
        /// <param name="addressTranslator">地址翻译器</param>
        /// <param name="endian">端格式</param>
        public WriteSingleCoilModbusInputStruct(byte slaveAddress, string startAddress, object writeValue,
            ModbusTranslatorBase addressTranslator, Endian endian)
        {
            SlaveAddress = slaveAddress;
            var translateAddress = addressTranslator.AddressTranslate(startAddress, false, true);
            FunctionCode = (byte)translateAddress.Area;
            StartAddress = (ushort)translateAddress.Address;
            var writeByteValue =
                FunctionCode == (byte)ModbusProtocolFunctionCode.WriteSingleCoil
                    ? ((bool)writeValue
                        ? new byte[] { 0xFF, 0x00 }
                        : new byte[] { 0x00, 0x00 })
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
    public class WriteSingleCoilModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="writeValue">写入的数据</param>
        public WriteSingleCoilModbusOutputStruct(byte slaveAddress, byte functionCode,
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
    public class WriteSingleCoilModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (WriteSingleCoilModbusInputStruct)message;
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
            var returnValue = functionCode == (byte)ModbusProtocolFunctionCode.WriteSingleCoil
                ? (object)(writeValue == 0xFF00) : writeValue;
            return new WriteSingleCoilModbusOutputStruct(slaveAddress, functionCode, startAddress,
                returnValue);
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
            {8, "MEMORY_PARITY_ERROR" },
            {10, "GATEWAY_PATH_UNAVAILABLE"},
            {11, "GATEWAY_TARGET_DEVICE_FAILED_TO_RESPOND"},
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