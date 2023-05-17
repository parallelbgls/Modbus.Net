using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtocolUnit = Modbus.Net.ProtocolUnit<byte[], byte[]>;

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
            if (writeByteValue.Length % 2 == 1) writeByteValue = writeByteValue.ToList().Append<byte>(0).ToArray();
            WriteCount =
                (ushort)(writeByteValue.Length / addressTranslator.GetAreaByteLength(translateAddress.AreaString));
            WriteByteCount = (byte)writeByteValue.Length;
            WriteValue = writeByteValue;
            translateAddress = ((ModbusTranslatorBase)addressTranslator).AddressTranslate(startAddress, false, WriteCount == 1);
            FunctionCode = (byte)translateAddress.Area;
            StartAddress = (ushort)translateAddress.Address;
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
            byte[] formattingBytes;
            if (r_message.FunctionCode == (byte)ModbusProtocolFunctionCode.WriteSingleCoil || r_message.FunctionCode == (byte)ModbusProtocolFunctionCode.WriteSingleRegister)
            {
                formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode,
                    r_message.StartAddress, dataValue);
            }
            else
            {
                formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode,
                    r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, dataValue);
            }

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
            if (functionCode == (byte)ModbusProtocolFunctionCode.WriteSingleCoil || functionCode == (byte)ModbusProtocolFunctionCode.WriteSingleRegister)
            {
                writeCount = 1;
            }
            return new WriteDataModbusOutputStruct(slaveAddress, functionCode, startAddress,
                writeCount);
        }
    }

    #endregion

    #region 读异常信息

    /// <summary>
    ///     读异常输入
    /// </summary>
    public class ReadExceptionStatusModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        public ReadExceptionStatusModbusInputStruct(byte slaveAddress)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusSerialPortOnlyFunctionCode.ReadExceptionStatus;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; }
    }

    /// <summary>
    ///     读异常输出
    /// </summary>
    public class ReadExceptionStatusModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="outputData">异常信息</param>
        public ReadExceptionStatusModbusOutputStruct(byte slaveAddress, byte functionCode,
            byte outputData)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            OutputData = outputData;
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
        ///     异常信息
        /// </summary>
        public byte OutputData { get; private set; }
    }

    /// <summary>
    ///     读异常协议
    /// </summary>
    public class ReadExceptionStatusModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ReadExceptionStatusModbusInputStruct)message;
            byte[] formattingBytes;

            formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode);

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
            var outputData = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            return new ReadExceptionStatusModbusOutputStruct(slaveAddress, functionCode, outputData);
        }
    }

    #endregion

    #region 诊断

    /// <summary>
    ///     诊断输入
    /// </summary>
    public class DiagnoticsModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="subFunction">诊断码</param>
        /// <param name="data">诊断内容</param>
        public DiagnoticsModbusInputStruct(byte slaveAddress, ushort subFunction, ushort[] data)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusSerialPortOnlyFunctionCode.Diagnostics;
            SubFunction = subFunction;
            Data = data.Clone() as ushort[];
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
        ///     诊断码
        /// </summary>
        public ushort SubFunction { get; }

        /// <summary>
        ///     诊断内容
        /// </summary>
        public ushort[] Data { get; }
    }

    /// <summary>
    ///     诊断输出
    /// </summary>
    public class DiagnoticsModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="subFunction">诊断码</param>
        /// <param name="data">诊断内容</param>
        public DiagnoticsModbusOutputStruct(byte slaveAddress, byte functionCode,
            ushort subFunction, ushort[] data)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            SubFunction = subFunction;
            Data = data;
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
        ///     诊断码
        /// </summary>
        public ushort SubFunction { get; }

        /// <summary>
        ///     诊断内容
        /// </summary>
        public ushort[] Data { get; }
    }

    /// <summary>
    ///     诊断协议
    /// </summary>
    public class DiagnoticsModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (DiagnoticsModbusInputStruct)message;

            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode,
                r_message.SubFunction, r_message.Data);

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
            var subFunction = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var dataValueBytes = new byte[subFunction * 2];
            Array.Copy(messageBytes, 3, dataValueBytes, 0, subFunction * 2);
            var dataValue = ValueHelper.GetInstance(Endian).ByteArrayToDestinationArray<ushort>(dataValueBytes, subFunction);
            return new DiagnoticsModbusOutputStruct(slaveAddress, functionCode, subFunction, dataValue);
        }
    }

    #endregion

    #region 读事件计数

    /// <summary>
    ///     读事件计数输入
    /// </summary>
    public class GetCommEventCounterModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        public GetCommEventCounterModbusInputStruct(byte slaveAddress)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusSerialPortOnlyFunctionCode.GetCommEventCounter;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; }
    }

    /// <summary>
    ///     读事件计数输出
    /// </summary>
    public class GetCommEventCounterModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="status">状态</param>
        /// <param name="eventCount">事件个数</param>
        public GetCommEventCounterModbusOutputStruct(byte slaveAddress, byte functionCode,
            ushort status, ushort eventCount)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            Status = status;
            EventCount = eventCount;
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
        ///     状态
        /// </summary>
        public ushort Status { get; }

        /// <summary>
        ///     事件个数
        /// </summary>
        public ushort EventCount { get; }
    }

    /// <summary>
    ///     读事件计数协议
    /// </summary>
    public class GetCommEventCounterModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (GetCommEventCounterModbusInputStruct)message;

            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode);

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
            var status = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var eventCount = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            return new GetCommEventCounterModbusOutputStruct(slaveAddress, functionCode, status, eventCount);
        }
    }

    #endregion

    #region 读事件

    /// <summary>
    ///     读事件输入
    /// </summary>
    public class GetCommEventLogModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        public GetCommEventLogModbusInputStruct(byte slaveAddress)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusSerialPortOnlyFunctionCode.GetCommEventLog;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; }
    }

    /// <summary>
    ///     诊断输出
    /// </summary>
    public class GetCommEventLogModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="byteCount">字节个数</param>
        /// <param name="status">状态码</param>
        /// <param name="eventCount">事件个数</param>
        /// <param name="messageCount">消息个数</param>
        /// <param name="events">事件信息</param>
        public GetCommEventLogModbusOutputStruct(byte slaveAddress, byte functionCode,
            byte byteCount, ushort status, ushort eventCount, ushort messageCount, byte[] events)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            ByteCount = byteCount;
            Status = status;
            EventCount = eventCount;
            MessageCount = messageCount;
            Events = events;
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
        ///     字节个数
        /// </summary>
        public byte ByteCount { get; private set; }

        /// <summary>
        ///     状态码
        /// </summary>
        public ushort Status { get; private set; }

        /// <summary>
        ///     事件个数
        /// </summary>
        public ushort EventCount { get; private set; }

        /// <summary>
        ///     消息个数
        /// </summary>
        public ushort MessageCount { get; private set; }

        /// <summary>
        ///     事件信息
        /// </summary>
        public byte[] Events { get; private set; }
    }

    /// <summary>
    ///     诊断协议
    /// </summary>
    public class GetCommEventLogModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (GetCommEventLogModbusInputStruct)message;

            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode);

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
            var byteCount = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var status = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var eventCount = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var messageCount = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var events = new byte[byteCount - 6];
            Array.Copy(messageBytes, 9, events, 0, byteCount - 6);
            return new GetCommEventLogModbusOutputStruct(slaveAddress, functionCode, byteCount, status, eventCount, messageCount, events);
        }
    }

    #endregion

    #region 报告从站ID

    /// <summary>
    ///     报告从站ID输入
    /// </summary>
    public class ReportSlaveIdModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        public ReportSlaveIdModbusInputStruct(byte slaveAddress)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusSerialPortOnlyFunctionCode.ReportSlaveID;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; }

        /// <summary>
        ///     功能码
        /// </summary>
        public byte FunctionCode { get; }
    }

    /// <summary>
    ///     报告从站ID输出
    /// </summary>
    public class ReportSlaveIdModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="byteCount">字节个数</param>
        /// <param name="slaveId">从站ID</param>
        /// <param name="runIndicatorStatus">运行状态</param>
        /// <param name="additionalData">附加信息</param>
        public ReportSlaveIdModbusOutputStruct(byte slaveAddress, byte functionCode,
            byte byteCount, byte slaveId, byte runIndicatorStatus, byte[] additionalData)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            ByteCount = byteCount;
            SlaveId = slaveId;
            RunIndicatorStatus = runIndicatorStatus;
            AdditionalData = additionalData;
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
        ///     字节个数
        /// </summary>
        public byte ByteCount { get; private set; }

        /// <summary>
        ///     从站ID
        /// </summary>
        public byte SlaveId { get; private set; }

        /// <summary>
        ///     运行状态
        /// </summary>
        public byte RunIndicatorStatus { get; private set; }

        /// <summary>
        ///     附加信息
        /// </summary>
        public byte[] AdditionalData { get; private set; }
    }

    /// <summary>
    ///     报告从站ID协议
    /// </summary>
    public class ReportSlaveIdModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ReportSlaveIdModbusInputStruct)message;

            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode);

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
            var byteCount = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var slaveId = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var runIndicatorStatus = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            var additionalData = new byte[byteCount - 2];
            Array.Copy(messageBytes, 5, additionalData, 0, byteCount - 2);
            return new ReportSlaveIdModbusOutputStruct(slaveAddress, functionCode, byteCount, slaveId, runIndicatorStatus, additionalData);
        }
    }

    #endregion

    #region 读文件记录

    /// <summary>
    ///     读文件输入数据定义
    /// </summary>
    public class ReadFileRecordInputDef
    {
        /// <summary>
        ///     引用类型，Modbus里固定为6
        /// </summary>
        public byte RefrenceType { get; } = 6;
        /// <summary>
        ///     文件计数
        /// </summary>
        public ushort FileNumber { get; set; }
        /// <summary>
        ///     记录计数
        /// </summary>
        public ushort RecordNumber { get; set; }
        /// <summary>
        ///     记录长度
        /// </summary>
        public ushort RecordLength { get; set; }
    }

    /// <summary>
    ///     读文件输出数据定义
    /// </summary>
    public class ReadFileRecordOutputDef
    {
        /// <summary>
        ///     返回长度
        /// </summary>
        public byte ResponseLength { get; set; }
        /// <summary>
        ///     引用类型
        /// </summary>
        public byte RefrenceType { get; set; }
        /// <summary>
        ///     记录数据
        /// </summary>
        public ushort[] RecordData { get; set; }
    }

    /// <summary>
    ///     读文件记录输入
    /// </summary>
    public class ReadFileRecordModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="recordDefs">读文件记录定义</param>
        public ReadFileRecordModbusInputStruct(byte slaveAddress, ReadFileRecordInputDef[] recordDefs)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusProtocolFunctionCode.ReadFileRecord;
            ByteCount = (byte)(recordDefs.Count() * 7);
            RecordDefs = recordDefs.Clone() as ReadFileRecordInputDef[];
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
        ///     字节长度
        /// </summary>
        public byte ByteCount { get; }

        /// <summary>
        ///     记录定义
        /// </summary>
        public ReadFileRecordInputDef[] RecordDefs { get; }
    }

    /// <summary>
    ///     读文件记录输出
    /// </summary>
    public class ReadFileRecordModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="byteCount">字节个数</param>
        /// <param name="recordDefs">文件记录返回结果</param>
        public ReadFileRecordModbusOutputStruct(byte slaveAddress, byte functionCode,
            byte byteCount, ReadFileRecordOutputDef[] recordDefs)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            ByteCount = byteCount;
            RecordDefs = recordDefs;
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
        ///     字节个数
        /// </summary>
        public byte ByteCount { get; private set; }

        /// <summary>
        ///     输出记录
        /// </summary>
        public ReadFileRecordOutputDef[] RecordDefs { get; private set; }
    }

    /// <summary>
    ///     读文件记录协议
    /// </summary>
    public class ReadFileRecordModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ReadFileRecordModbusInputStruct)message;

            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode);

            foreach (var def in r_message.RecordDefs)
            {
                formattingBytes = Format(formattingBytes, def.RefrenceType, def.FileNumber, def.RecordNumber, def.RecordLength);
            }

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
            var byteCount = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            List<ReadFileRecordOutputDef> records = new List<ReadFileRecordOutputDef>();
            for (int i = 0; i < byteCount; i++)
            {
                ReadFileRecordOutputDef record = new ReadFileRecordOutputDef();
                var fileResponseLength = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
                record.ResponseLength = fileResponseLength;
                i++;
                var fileReferenceType = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
                record.RefrenceType = fileReferenceType;
                i++;
                var recordLength = (fileResponseLength - 1) / 2;
                ushort[] recordContent = new ushort[recordLength];
                for (int j = 0; j < recordLength; j++)
                {
                    recordContent[j] = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
                    i += 2;
                }
                record.RecordData = recordContent;
                records.Add(record);
            }
            return new ReadFileRecordModbusOutputStruct(slaveAddress, functionCode, byteCount, records.ToArray());
        }
    }

    #endregion

    #region 写文件记录

    /// <summary>
    ///     写文件记录输入定义
    /// </summary>
    public class WriteFileRecordInputDef
    {
        /// <summary>
        ///     引用类型，Modbus固定为6
        /// </summary>
        public byte RefrenceType { get; } = 6;
        /// <summary>
        ///     文件计数
        /// </summary>
        public ushort FileNumber { get; set; }
        /// <summary>
        ///     记录计数
        /// </summary>
        public ushort RecordNumber { get; set; }
        /// <summary>
        ///     记录长度
        /// </summary>
        public ushort RecordLength => RecordData != null ? (ushort)RecordData.Length : (ushort)0;
        /// <summary>
        ///     记录数据
        /// </summary>
        public ushort[] RecordData { get; set; }
    }

    /// <summary>
    ///     写文件记录输出定义
    /// </summary>
    public class WriteFileRecordOutputDef
    {
        /// <summary>
        ///     引用类型
        /// </summary>
        public byte RefrenceType { get; set; }
        /// <summary>
        ///     文件计数
        /// </summary>
        public ushort FileNumber { get; set; }
        /// <summary>
        ///     记录计数
        /// </summary>
        public ushort RecordNumber { get; set; }
        /// <summary>
        ///     记录长度
        /// </summary>
        public ushort RecordLength { get; set; }
        /// <summary>
        ///     记录数据
        /// </summary>
        public ushort[] RecordData { get; set; }
    }

    /// <summary>
    ///     写文件记录输入
    /// </summary>
    public class WriteFileRecordModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="writeRecords">写文件记录内容</param>
        public WriteFileRecordModbusInputStruct(byte slaveAddress, WriteFileRecordInputDef[] writeRecords)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusProtocolFunctionCode.WriteFileRecord;
            byte count = 0;
            foreach (var writeRecord in writeRecords)
            {
                count += (byte)(writeRecord.RecordData.Length * 2 + 7);
            }
            ByteCount = count;
            WriteRecords = writeRecords.Clone() as WriteFileRecordInputDef[];
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
        ///     字节个数
        /// </summary>
        public byte ByteCount { get; private set; }

        /// <summary>
        ///     写记录数据
        /// </summary>
        public WriteFileRecordInputDef[] WriteRecords { get; private set; }
    }

    /// <summary>
    ///     写文件记录输出
    /// </summary>
    public class WriteFileRecordModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="byteCount">字节个数</param>
        /// <param name="writeRecords">写文件记录返回</param>
        public WriteFileRecordModbusOutputStruct(byte slaveAddress, byte functionCode,
            byte byteCount, WriteFileRecordOutputDef[] writeRecords)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            ByteCount = byteCount;
            WriteRecords = writeRecords;
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
        ///     字节个数
        /// </summary>
        public byte ByteCount { get; private set; }

        /// <summary>
        ///     写记录数据
        /// </summary>
        public WriteFileRecordOutputDef[] WriteRecords { get; private set; }
    }

    /// <summary>
    ///     写文件记录协议
    /// </summary>
    public class WriteFileRecordModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (WriteFileRecordModbusInputStruct)message;

            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode, r_message.ByteCount);

            foreach (var record in r_message.WriteRecords)
            {
                formattingBytes = Format(formattingBytes, record.RefrenceType, record.FileNumber, record.RecordNumber, record.RecordLength, record.RecordData);
            }

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
            var byteCount = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            List<WriteFileRecordOutputDef> records = new List<WriteFileRecordOutputDef>();
            for (int i = 0; i < byteCount; i++)
            {
                WriteFileRecordOutputDef record = new WriteFileRecordOutputDef();
                var fileReferenceType = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
                record.RefrenceType = fileReferenceType;
                i++;
                var fileNumber = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
                record.FileNumber = fileNumber;
                i += 2;
                var fileRecordNumber = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
                record.RecordNumber = fileRecordNumber;
                i += 2;
                var fileRecordLength = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
                record.RecordLength = fileRecordLength;
                i += 2;
                ushort[] recordContent = new ushort[fileRecordLength];
                for (int j = 0; j < fileRecordLength; j++)
                {
                    recordContent[j] = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
                    i += 2;
                }
                record.RecordData = recordContent;
                records.Add(record);
            }
            return new WriteFileRecordModbusOutputStruct(slaveAddress, functionCode, byteCount, records.ToArray());
        }
    }

    #endregion

    #region 写掩码

    /// <summary>
    ///     写文件记录输入
    /// </summary>
    public class MaskWriteRegisterModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="referenceAddress">地址索引</param>
        /// <param name="andMask">与掩码</param>
        /// <param name="orMask">或掩码</param>
        public MaskWriteRegisterModbusInputStruct(byte slaveAddress, ushort referenceAddress, ushort andMask, ushort orMask)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusProtocolFunctionCode.MaskWriteRegister;
            ReferenceAddress = referenceAddress;
            AndMask = andMask;
            OrMask = orMask;
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
        ///     地址索引
        /// </summary>
        public ushort ReferenceAddress { get; }

        /// <summary>
        ///     与掩码
        /// </summary>
        public ushort AndMask { get; }

        /// <summary>
        ///     或掩码
        /// </summary>
        public ushort OrMask { get; }
    }

    /// <summary>
    ///     写文件记录输出
    /// </summary>
    public class MaskWriteRegisterModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="referenceAddress">地址索引</param>
        /// <param name="andMask">与掩码</param>
        /// <param name="orMask">或掩码</param>
        public MaskWriteRegisterModbusOutputStruct(byte slaveAddress, byte functionCode,
            ushort referenceAddress, ushort andMask, ushort orMask)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            ReferenceAddress = referenceAddress;
            AndMask = andMask;
            OrMask = orMask;
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
        ///     地址索引
        /// </summary>
        public ushort ReferenceAddress { get; }

        /// <summary>
        ///     与掩码
        /// </summary>
        public ushort AndMask { get; }

        /// <summary>
        ///     或掩码
        /// </summary>
        public ushort OrMask { get; }
    }

    /// <summary>
    ///     写文件记录协议
    /// </summary>
    public class MaskWriteRegisterModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (MaskWriteRegisterModbusInputStruct)message;

            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode, r_message.ReferenceAddress, r_message.AndMask, r_message.OrMask);

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
            var referenceAddress = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var andMask = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var orMask = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            return new MaskWriteRegisterModbusOutputStruct(slaveAddress, functionCode, referenceAddress, andMask, orMask);
        }
    }

    #endregion

    #region 读写多个输入寄存器

    /// <summary>
    ///     读写多个输入寄存器输入
    /// </summary>
    public class ReadWriteMultipleRegistersModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="readStartingAddress">读起始地址</param>
        /// <param name="quantityToRead">读个数</param>
        /// <param name="writeStartingAddress">写起始地址</param>
        /// <param name="writeValues">写内容</param>
        public ReadWriteMultipleRegistersModbusInputStruct(byte slaveAddress, ushort readStartingAddress, ushort quantityToRead, ushort writeStartingAddress, ushort[] writeValues)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusProtocolFunctionCode.ReadWriteMultipleRegister;
            ReadStartingAddress = readStartingAddress;
            QuantityToRead = quantityToRead;
            WriteStartingAddress = writeStartingAddress;
            QuantityToWrite = (ushort)writeValues.Count();
            WriteByteCount = (ushort)(QuantityToWrite * 2);
            WriteValues = writeValues.Clone() as ushort[];
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
        ///     读起始地址
        /// </summary>
        public ushort ReadStartingAddress { get; }

        /// <summary>
        ///     读个数
        /// </summary>
        public ushort QuantityToRead { get; }

        /// <summary>
        ///     写起始地址
        /// </summary>
        public ushort WriteStartingAddress { get; }

        /// <summary>
        ///     写个数
        /// </summary>
        public ushort QuantityToWrite { get; }

        /// <summary>
        ///     写字节个数
        /// </summary>
        public ushort WriteByteCount { get; }

        /// <summary>
        ///     写数据
        /// </summary>
        public ushort[] WriteValues { get; }
    }

    /// <summary>
    ///     读写多个输入寄存器输出
    /// </summary>
    public class ReadWriteMultipleRegistersModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="byteCount">获取的字节数</param>
        /// <param name="readRegisterValues">读取的寄存器内容</param>
        public ReadWriteMultipleRegistersModbusOutputStruct(byte slaveAddress, byte functionCode, byte byteCount, ushort[] readRegisterValues)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            ByteCount = byteCount;
            ReadRegisterValues = readRegisterValues;
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
        ///     字节个数
        /// </summary>
        public byte ByteCount { get; private set; }

        /// <summary>
        ///     读数据内容
        /// </summary>
        public ushort[] ReadRegisterValues { get; private set; }
    }

    /// <summary>
    ///     读写多个输入寄存器协议
    /// </summary>
    public class ReadWriteMultipleRegistersModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ReadWriteMultipleRegistersModbusInputStruct)message;

            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode, r_message.ReadStartingAddress,
                r_message.QuantityToRead, r_message.WriteStartingAddress, r_message.QuantityToWrite,
                r_message.WriteByteCount, r_message.WriteValues);

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
            var byteCount = ValueHelper.GetInstance(Endian).GetByte(messageBytes, ref flag);
            ushort[] readValues = new ushort[byteCount / 2];
            for (int i = 0; i < byteCount / 2; i++)
            {
                readValues[i] = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            }
            return new ReadWriteMultipleRegistersModbusOutputStruct(slaveAddress, functionCode, byteCount, readValues);
        }
    }

    #endregion

    #region 读FIFO队列

    /// <summary>
    ///     读写多个输入寄存器输入
    /// </summary>
    public class ReadFIFOQueueModbusInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="fifoPointerAddress">FIFO队列地址索引</param>
        public ReadFIFOQueueModbusInputStruct(byte slaveAddress, ushort fifoPointerAddress)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = (byte)ModbusProtocolFunctionCode.ReadFIFOQueue;
            FIFOPointerAddress = fifoPointerAddress;
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
        ///     FIFO地址
        /// </summary>
        public ushort FIFOPointerAddress { get; }
    }

    /// <summary>
    ///     读写多个输入寄存器输出
    /// </summary>
    public class ReadFIFOQueueModbusOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="byteCount">获取的字节数</param>
        /// <param name="fifoCount">FIFO个数</param>
        /// <param name="fifoValueRegister">FIFO值</param>
        public ReadFIFOQueueModbusOutputStruct(byte slaveAddress, byte functionCode, ushort byteCount, ushort fifoCount, ushort[] fifoValueRegister)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            ByteCount = byteCount;
            FIFOCount = fifoCount;
            FIFOValueRegister = fifoValueRegister;
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
        ///     字节个数
        /// </summary>
        public ushort ByteCount { get; private set; }

        /// <summary>
        ///     FIFO个数
        /// </summary>
        public ushort FIFOCount { get; private set; }

        /// <summary>
        ///     FIFO内容
        /// </summary>
        public ushort[] FIFOValueRegister { get; private set; }
    }

    /// <summary>
    ///     读写多个输入寄存器协议
    /// </summary>
    public class ReadFIFOQueueModbusProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">写寄存器参数</param>
        /// <returns>写寄存器协议核心</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ReadFIFOQueueModbusInputStruct)message;

            var formattingBytes = Format(r_message.SlaveAddress, r_message.FunctionCode, r_message.FIFOPointerAddress);

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
            var byteCount = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            var fifoCount = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            ushort[] readValues = new ushort[fifoCount];
            for (int i = 0; i < fifoCount; i++)
            {
                readValues[i] = ValueHelper.GetInstance(Endian).GetUShort(messageBytes, ref flag);
            }
            return new ReadFIFOQueueModbusOutputStruct(slaveAddress, functionCode, byteCount, fifoCount, readValues);
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
            {11, "GATEWAY_TARGET_DEVICE_FAILED_TO_RESPOND"}
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