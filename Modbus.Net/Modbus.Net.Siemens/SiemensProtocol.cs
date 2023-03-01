using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子数据类型
    /// </summary>
    public enum SiemensTypeCode : byte
    {
        /// <summary>
        ///     布尔
        /// </summary>
        Bool = 0x01,

        /// <summary>
        ///     字节
        /// </summary>
        Byte = 0x02,

        /// <summary>
        ///     字
        /// </summary>
        Word = 0x03,

        /// <summary>
        ///     双字
        /// </summary>
        DWord = 0x04,

        /// <summary>
        ///     计数器
        /// </summary>
        C = 0x1E,

        /// <summary>
        ///     计时器
        /// </summary>
        T = 0x1F,

        /// <summary>
        ///     高速计数器
        /// </summary>
        HC = 0x20
    }

    /// <summary>
    ///     西门子通讯报错信息
    /// </summary>
    public enum SiemensAccessResult : byte
    {
        /// <summary>
        ///     无错误
        /// </summary>
        NoError = 0xFF,

        /// <summary>
        ///     硬件错误
        /// </summary>
        HardwareFault = 0x01,

        /// <summary>
        ///     非法对象访问（Area错误）
        /// </summary>
        IllegalObjectAccess = 0x03,

        /// <summary>
        ///     非法地址访问
        /// </summary>
        InvalidAddress = 0x05,

        /// <summary>
        ///     不支持的数据类型
        /// </summary>
        DataTypeNotSupport = 0x06,

        /// <summary>
        ///     对象不存在或长度超出允许范围
        /// </summary>
        ObjNotExistOrLengthError = 0x0A
    }

    /// <summary>
    ///     西门子数据访问类型
    /// </summary>
    public enum SiemensDataType : byte
    {
        /// <summary>
        ///     错误
        /// </summary>
        Error = 0x00,

        /// <summary>
        ///     比特位访问
        /// </summary>
        BitAccess = 0x03,

        /// <summary>
        ///     一般访问
        /// </summary>
        OtherAccess = 0x04
    }

    /// <summary>
    ///     西门子协议
    /// </summary>
    public abstract class SiemensProtocol : BaseProtocol
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        protected SiemensProtocol(byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress, Endian.BigEndianLsb)
        {
        }
    }

    #region 串口连接建立

    internal class ComCreateReferenceSiemensInputStruct : IInputStruct
    {
        public ComCreateReferenceSiemensInputStruct(byte slaveAddress, byte masterAddress)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
        }

        public byte SlaveAddress { get; set; }
        public byte MasterAddress { get; set; }
    }

    internal class ComCreateReferenceSiemensOutputStruct : IOutputStruct
    {
        public ComCreateReferenceSiemensOutputStruct(byte slaveAddress, byte masterAddress, byte confirmMessage)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
            ConfirmMessage = confirmMessage;
        }

        public byte SlaveAddress { get; set; }
        public byte MasterAddress { get; set; }
        public byte ConfirmMessage { get; set; }
    }

    [SpecialProtocolUnit]
    internal class ComCreateReferenceSiemensProtocol : ProtocolUnit
    {
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ComCreateReferenceSiemensInputStruct)message;
            var crc = (r_message.SlaveAddress + r_message.MasterAddress + 0x49) % 256;
            return Format((byte)0x10, r_message.SlaveAddress, r_message.MasterAddress, (byte)0x49, (byte)crc,
                (byte)0x16);
        }

        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 1;
            var masterAddress = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            var slaveAddress = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            var confirmMessage = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            return new ComCreateReferenceSiemensOutputStruct(slaveAddress, masterAddress, confirmMessage);
        }
    }

    #endregion

    #region 以太网建立连接

    internal class CreateReferenceSiemensInputStruct : IInputStruct
    {
        public byte TdpuSize;

        public ushort TsapDst;

        public ushort TsapSrc;

        public CreateReferenceSiemensInputStruct(byte tdpuSize, ushort srcTsap, ushort dstTsap)
        {
            TdpuSize = tdpuSize;
            TsapSrc = srcTsap;
            TsapDst = dstTsap;
        }
    }

    internal class CreateReferenceSiemensOutputStruct : IOutputStruct
    {
        public CreateReferenceSiemensOutputStruct(byte tdpuSize, ushort srcTsap, ushort dstTsap)
        {
            TdpuSize = tdpuSize;
            TsapSrc = srcTsap;
            TsapDst = dstTsap;
        }

        public byte TdpuSize { get; }
        public ushort TsapSrc { get; }
        public ushort TsapDst { get; }
    }

    [SpecialProtocolUnit]
    internal class CreateReferenceSiemensProtocol : ProtocolUnit
    {
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (CreateReferenceSiemensInputStruct)message;
            const ushort head = 0x0300;
            const ushort len = 0x0016;
            const byte contentLen = 0x11;
            const byte typeCode = 0xe0;
            const ushort dstRef = 0x0000;
            const ushort srcRef = 0x000c;
            const byte reserved = 0x00;
            const ushort tdpuSizeCode = 0xc001;
            var tdpuSizeContent = r_message.TdpuSize;
            const ushort srcTsapCode = 0xc102;
            var srcTsapContent = r_message.TsapSrc;
            const ushort dstTsapCode = 0xc202;
            var dstTsapContent = r_message.TsapDst;
            return Format(head, len, contentLen, typeCode, dstRef, srcRef, reserved, tdpuSizeCode, tdpuSizeContent,
                srcTsapCode, srcTsapContent, dstTsapCode, dstTsapContent);
        }

        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 11;
            byte tdpuSize = 0;
            ushort srcTsap = 0, dstTsap = 0;
            switch (messageBytes[pos])
            {
                case 0xc0:
                    {
                        pos += 2;
                        tdpuSize = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
                        break;
                    }
                case 0xc1:
                    {
                        pos += 2;
                        srcTsap = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
                        break;
                    }
                case 0xc2:
                    {
                        pos += 2;
                        dstTsap = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
                        break;
                    }
            }
            return new CreateReferenceSiemensOutputStruct(tdpuSize, srcTsap, dstTsap);
        }
    }

    #endregion

    #region 串口消息确认 

    /// <summary>
    ///     串口消息确认输入
    /// </summary>
    public class ComConfirmMessageSiemensInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public ComConfirmMessageSiemensInputStruct(byte slaveAddress, byte masterAddress)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; set; }

        /// <summary>
        ///     主站号
        /// </summary>
        public byte MasterAddress { get; set; }
    }

    /// <summary>
    ///     串口消息确认输出
    /// </summary>
    public class ComConfirmMessageSiemensOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="confirmByte">确认字节</param>
        public ComConfirmMessageSiemensOutputStruct(byte confirmByte)
        {
            ConfirmByte = confirmByte;
        }

        /// <summary>
        ///     确认字节
        /// </summary>
        public byte ConfirmByte { get; set; }
    }

    /// <summary>
    ///     串口消息确认协议
    /// </summary>
    [SpecialProtocolUnit]
    public class ComConfirmMessageSiemensProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">输入参数</param>
        /// <returns>格式化数据</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ComConfirmMessageSiemensInputStruct)message;
            var crc = r_message.SlaveAddress + r_message.MasterAddress + 0x5c % 256;
            return Format((byte)0x10, r_message.SlaveAddress, r_message.MasterAddress, (byte)0x5c, (byte)crc,
                (byte)0x16);
        }

        /// <summary>
        ///     反格式化
        /// </summary>
        /// <param name="messageBytes">设备返回的数据</param>
        /// <param name="pos">当前反格式化的位置</param>
        /// <returns>输出数据</returns>
        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            var confirmByte = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            return new ComConfirmMessageSiemensOutputStruct(confirmByte);
        }
    }

    #endregion

    #region 以太网连接确认

    internal class EstablishAssociationSiemensInputStruct : IInputStruct
    {
        public EstablishAssociationSiemensInputStruct(ushort pduRef, ushort maxCalling, ushort maxCalled, ushort maxPdu)
        {
            PduRef = pduRef;
            MaxCalling = maxCalling;
            MaxCalled = maxCalled;
            MaxPdu = maxPdu;
        }

        public ushort PduRef { get; }
        public ushort MaxCalling { get; }
        public ushort MaxCalled { get; }
        public ushort MaxPdu { get; }
    }

    internal class EstablishAssociationSiemensOutputStruct : IOutputStruct
    {
        public EstablishAssociationSiemensOutputStruct(ushort pduRef, ushort maxCalling, ushort maxCalled,
            ushort maxPdu)
        {
            PduRef = pduRef;
            MaxCalling = maxCalling;
            MaxCalled = maxCalled;
            MaxPdu = maxPdu;
        }

        public ushort PduRef { get; }
        public ushort MaxCalling { get; }
        public ushort MaxCalled { get; }
        public ushort MaxPdu { get; }
    }

    internal class EstablishAssociationSiemensProtocol : ProtocolUnit
    {
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (EstablishAssociationSiemensInputStruct)message;
            const byte protoId = 0x32;
            const byte rosctr = 0x01;
            const ushort redId = 0x0000;
            var pduRef = r_message.PduRef;
            const ushort parLg = 0x0008;
            const ushort datLg = 0x0000;
            const byte serviceId = 0xf0;
            const byte reserved = 0x00;
            var maxCalling = r_message.MaxCalling;
            var maxCalled = r_message.MaxCalled;
            var maxPdu = r_message.MaxPdu;
            return Format(new byte[7], protoId, rosctr, redId, pduRef, parLg, datLg, serviceId, reserved, maxCalling,
                maxCalled, maxPdu);
        }

        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 4;
            var pduRef = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 14;
            var maxCalling = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            var maxCalled = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            var maxPdu = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            return new EstablishAssociationSiemensOutputStruct(pduRef, maxCalling, maxCalled, maxPdu);
        }
    }

    #endregion

    #region 读数据请求

    /// <summary>
    ///     读数据输入
    /// </summary>
    public class ReadRequestSiemensInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="pduRef">报文索引</param>
        /// <param name="getType">获取数据类型</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getCount">获取个数</param>
        /// <param name="addressTranslator">地址转换器</param>
        public ReadRequestSiemensInputStruct(byte slaveAddress, byte masterAddress, ushort pduRef,
            SiemensTypeCode getType, string startAddress, ushort getCount, AddressTranslator addressTranslator)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
            PduRef = pduRef;
            TypeCode = (byte)getType;
            var address = addressTranslator.AddressTranslate(startAddress, true);
            Offset = address.Address;
            var area = address.Area;
            Area = (byte)(area % 256);
            DbBlock = Area == 0x84 ? (ushort)(area / 256) : (ushort)0;
            NumberOfElements = getCount;
        }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; set; }

        /// <summary>
        ///     主站号
        /// </summary>
        public byte MasterAddress { get; set; }

        /// <summary>
        ///     报文索引
        /// </summary>
        public ushort PduRef { get; }

        /// <summary>
        ///     地址类型
        /// </summary>
        public byte TypeCode { get; }

        /// <summary>
        ///     读取的个数
        /// </summary>
        public ushort NumberOfElements { get; }

        /// <summary>
        ///     DB块
        /// </summary>
        public ushort DbBlock { get; }

        /// <summary>
        ///     区域
        /// </summary>
        public byte Area { get; }

        /// <summary>
        ///     起始偏移量
        /// </summary>
        public int Offset { get; }
    }

    /// <summary>
    ///     读数据输出
    /// </summary>
    public class ReadRequestSiemensOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="pduRef">报文索引</param>
        /// <param name="accessResult">访问结果</param>
        /// <param name="dataType">数据类型</param>
        /// <param name="getLength">获取个数</param>
        /// <param name="value">读取值</param>
        public ReadRequestSiemensOutputStruct(ushort pduRef, SiemensAccessResult accessResult, SiemensDataType dataType,
            ushort getLength, byte[] value)
        {
            PduRef = pduRef;
            AccessResult = accessResult;
            DataType = dataType;
            GetLength = getLength;
            GetValue = value;
        }

        /// <summary>
        ///     报文索引
        /// </summary>
        public ushort PduRef { get; }

        /// <summary>
        ///     访问结果
        /// </summary>
        public SiemensAccessResult AccessResult { get; }

        /// <summary>
        ///     数据类型
        /// </summary>
        public SiemensDataType DataType { get; }

        /// <summary>
        ///     获取个数
        /// </summary>
        public ushort GetLength { get; }

        /// <summary>
        ///     读取值
        /// </summary>
        public byte[] GetValue { get; }
    }

    /// <summary>
    ///     读数据协议
    /// </summary>
    public class ReadRequestSiemensProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">输入参数</param>
        /// <returns>格式化数据</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ReadRequestSiemensInputStruct)message;
            var slaveAddress = r_message.SlaveAddress;
            var masterAddress = r_message.MasterAddress;
            const byte protoId = 0x32;
            const byte rosctr = 0x01;
            const ushort redId = 0x0000;
            var pduRef = r_message.PduRef;
            const ushort parLg = 14; // 参数字节数（2+12的倍数），目前仅为14
            const ushort datLg = 0; // 数据字节数
            const byte serviceId = 0x04;
            const byte numberOfVariables = 1;
            const byte variableSpec = 0x12;
            const byte vAddrLg = 0x0A;
            const byte syntaxId = 0x10;
            var type = r_message.TypeCode;
            var numberOfElements = r_message.NumberOfElements;
            var dbBlock = r_message.DbBlock;
            var area = r_message.Area;
            var offsetBit = r_message.Offset * 8;
            var offsetBitBytes = BigEndianValueHelper.Instance.GetBytes(offsetBit);
            return Format(new byte[4], slaveAddress, masterAddress, (byte)0x6c, protoId, rosctr, redId, pduRef, parLg,
                datLg, serviceId, numberOfVariables
                , variableSpec, vAddrLg, syntaxId, type, numberOfElements, dbBlock, area,
                offsetBitBytes.Skip(1).ToArray());
        }

        /// <summary>
        ///     反格式化
        /// </summary>
        /// <param name="messageBytes">设备返回的数据</param>
        /// <param name="pos">当前反格式化的位置</param>
        /// <returns>输出数据</returns>
        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 4;
            var pduRef = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 14;
            var accessResult = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            var dataType = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            var length = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            var byteLength = length / 8;
            var values = new byte[byteLength];
            Array.Copy(messageBytes, pos, values, 0, byteLength);
            return new ReadRequestSiemensOutputStruct(pduRef, (SiemensAccessResult)accessResult,
                (SiemensDataType)dataType, length, values);
        }
    }

    #endregion

    #region 写数据请求

    /// <summary>
    ///     写数据输入
    /// </summary>
    public class WriteRequestSiemensInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="pduRef">报文索引</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="writeValue">写入值</param>
        /// <param name="addressTranslator">地址转换器</param>
        public WriteRequestSiemensInputStruct(byte slaveAddress, byte masterAddress, ushort pduRef, string startAddress,
            object[] writeValue, AddressTranslator addressTranslator)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
            PduRef = pduRef;
            var address = addressTranslator.AddressTranslate(startAddress, true);
            Offset = address.Address;
            var area = address.Area;
            Area = (byte)(area % 256);
            DbBlock = Area == 0x84 ? (ushort)(area / 256) : (ushort)0;
            WriteValue = writeValue;
        }

        /// <summary>
        ///     从站地址
        /// </summary>
        public byte SlaveAddress { get; set; }

        /// <summary>
        ///     主站地址
        /// </summary>
        public byte MasterAddress { get; set; }

        /// <summary>
        ///     报文索引
        /// </summary>
        public ushort PduRef { get; }

        /// <summary>
        ///     DB块
        /// </summary>
        public ushort DbBlock { get; }

        /// <summary>
        ///     区域
        /// </summary>
        public byte Area { get; }

        /// <summary>
        ///     写入偏移量
        /// </summary>
        public int Offset { get; }

        /// <summary>
        ///     写入值
        /// </summary>
        public object[] WriteValue { get; }
    }

    /// <summary>
    ///     写数据输出
    /// </summary>
    public class WriteRequestSiemensOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="pduRef">报文索引</param>
        /// <param name="accessResult">访问结果</param>
        public WriteRequestSiemensOutputStruct(ushort pduRef, SiemensAccessResult accessResult)
        {
            PduRef = pduRef;
            AccessResult = accessResult;
        }

        /// <summary>
        ///     报文索引
        /// </summary>
        public ushort PduRef { get; }

        /// <summary>
        ///     访问结果
        /// </summary>
        public SiemensAccessResult AccessResult { get; }
    }

    /// <summary>
    ///     写数据协议
    /// </summary>
    public class WriteRequestSiemensProtocol : ProtocolUnit
    {
        /// <summary>
        ///     格式化
        /// </summary>
        /// <param name="message">输入参数</param>
        /// <returns>格式化数据</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (WriteRequestSiemensInputStruct)message;
            var valueBytes = BigEndianValueHelper.Instance.ObjectArrayToByteArray(r_message.WriteValue);
            var slaveAddress = r_message.SlaveAddress;
            var masterAddress = r_message.MasterAddress;
            const byte protoId = 0x32;
            const byte rosctr = 0x01;
            const ushort redId = 0x0000;
            var pduRef = r_message.PduRef;
            const ushort parLg = 14; // 参数字节数（2+12的倍数），目前仅为14
            var datLg = (ushort)(4 + valueBytes.Length); // 数据字节数
            const byte serviceId = 0x05;
            const byte numberOfVariables = 1;
            const byte variableSpec = 0x12;
            const byte vAddrLg = 0x0A;
            const byte syntaxId = 0x10;
            const byte typeR = (byte)SiemensTypeCode.Byte;
            var numberOfElements = (ushort)valueBytes.Length;
            var dbBlock = r_message.DbBlock;
            var area = r_message.Area;
            var offsetBit = r_message.Offset * 8;
            var offsetBitBytes = BigEndianValueHelper.Instance.GetBytes(offsetBit);
            const byte reserved = 0x00;
            const byte type = (byte)SiemensDataType.OtherAccess;
            var numberOfWriteBits = (ushort)(valueBytes.Length * 8);
            return Format(new byte[4], slaveAddress, masterAddress, (byte)0x7c, protoId, rosctr, redId, pduRef, parLg,
                datLg, serviceId, numberOfVariables
                , variableSpec, vAddrLg, syntaxId, typeR, numberOfElements, dbBlock, area,
                offsetBitBytes.Skip(1).ToArray(), reserved, type, numberOfWriteBits, valueBytes);
        }

        /// <summary>
        ///     反格式化
        /// </summary>
        /// <param name="messageBytes">设备返回的数据</param>
        /// <param name="pos">当前反格式化的位置</param>
        /// <returns>输出数据</returns>
        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            if (messageBytes.Length == 1)
            {
                var accessResult = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
                return new WriteRequestSiemensOutputStruct(0,
                    accessResult == 0xe5 ? SiemensAccessResult.NoError : SiemensAccessResult.InvalidAddress);
            }
            else
            {
                pos = 4;
                var pduRef = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
                pos = 14;
                var accessResult = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
                return new WriteRequestSiemensOutputStruct(pduRef, (SiemensAccessResult)accessResult);
            }
        }
    }

    #endregion

    /*
    #region 读时间请求
    public class ReadTimeSiemensInputStruct : IInputStruct
    {
        public ReadTimeSiemensInputStruct(ushort pduRef)
        {
            PduRef = pduRef;
        }

        public ushort PduRef { get; private set; }
    }

    public class ReadTimeSiemensOutputStruct : IOutputStruct
    {
        public ReadTimeSiemensOutputStruct(ushort pduRef, DateTime dateTime, TodClockStatus todClockStatus)
        {
            PduRef = pduRef;
            DateTime = dateTime;
            TodClockStatus = todClockStatus;
        }

        public ushort PduRef { get; private set; }
        public DateTime DateTime { get; private set; }
        public TodClockStatus TodClockStatus { get; private set; }
    }

    public class ReadTimeSiemensProtocol : ProtocolUnit
    {
        public override byte[] Format(IInputStruct message)
        {
            throw new NotImplementedException();
        }

        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region 写时间请求
    
    public class WriteTimeSiemensInputStruct : IInputStruct
    {
        public WriteTimeSiemensInputStruct(ushort pduRef, DateTime dateTime, TodClockStatus todClockStatus)
        {
            PduRef = pduRef;
            DateTime = dateTime;
            TodClockStatus = todClockStatus;
        }

        public ushort PduRef { get; private set; }
        public DateTime DateTime { get; private set; }
        public TodClockStatus TodClockStatus { get; private set; }
    }

    public class WriteTimeSiemensOutputStruct : IOutputStruct
    {
        public WriteTimeSiemensOutputStruct(ushort pduRef, byte errCod)
        {
            PduRef = pduRef;
            ErrCod = errCod;
        }

        public ushort PduRef { get; private set; }
        public byte ErrCod { get;private set; }
    }

    public class WriteTimeSiemensProtocol : ProtocolUnit
    {
        public override byte[] Format(IInputStruct message)
        {
            throw new NotImplementedException();
        }

        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
    */

    /// <summary>
    ///     西门子通讯报错信息
    /// </summary>
    public class SiemensProtocolErrorException : ProtocolErrorException
    {
        private static readonly Dictionary<int, string> ProtocolErrorDictionary = new Dictionary<int, string>
        {
            {0x00, "No Error"},
            {0x81, "Error in the application Id of the request"},
            {0x82, "Error in the object definition"},
            {0x83, "No recources available"},
            {0x84, "Error in the sructure of the service request"},
            {0x85, "Error in the communitcation equipment"},
            {0x87, "Access Error"},
            {0xD2, "OVS error"},
            {0xD4, "Diagnostic error"},
            {0xD6, "Protection system error"},
            {0xD8, "BuB error"},
            {0xEF, "Layer 2 specific error"}
        };

        private static readonly Dictionary<int, string> ProtocolErrorDetailDictionary = new Dictionary<int, string>
        {
            {0x8304, "Resource not available,\r\n there are no more resources available for application associations to be established" },
            {0x8104, "Context is not supported:\r\n -Error in PDU structure\r\n -Unknown service" },
            {0x8404, "Fatal error detected.\r\n Service or function aborted" },
            {0x8500, "PDU size error"}
        };

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="errCls">错误分类</param>
        /// <param name="errCod">错误码</param>
        public SiemensProtocolErrorException(int errCls, int errCod)
            : base((ProtocolErrorDictionary.ContainsKey(errCls)
                       ? ProtocolErrorDictionary[errCls]
                       : "Unknown error") + " \r\n " +
                   (ProtocolErrorDetailDictionary.ContainsKey(errCls * 256 + errCod)
                       ? ProtocolErrorDetailDictionary[errCls * 256 + errCod]
                       : "Unknown error detail"))
        {
            ErrorClass = errCls;
            ErrorCode = errCod;
        }

        /// <summary>
        ///     错误分类
        /// </summary>
        public int ErrorClass { get; }

        /// <summary>
        ///     错误码
        /// </summary>
        public int ErrorCode { get; }
    }
}