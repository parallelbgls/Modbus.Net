using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net.Siemens
{
    public enum SiemensTypeCode : byte
    {
        Bool = 0x01,
        Byte = 0x02,
        Word = 0x03,
        DWord = 0x04,
        C = 0x1E,
        T = 0x1F,
        HC = 0x20,
    };

    public enum SiemensAccessResult : byte
    {
        NoError = 0xFF,
        HardwareFault = 0x01,
        IllegalObjectAccess = 0x03,
        InvalidAddress = 0x05,
        DataTypeNotSupport = 0x06,
        ObjNotExistOrLengthError = 0x0A,
    };

    public enum SiemensDataType : byte
    {
        Error = 0x00,
        BitAccess = 0x03,
        OtherAccess = 0x04
    };

    public abstract class SiemensProtocal : BaseProtocal
    {

    }

    internal class CreateReferenceSiemensInputStruct : InputStruct
    {
        public CreateReferenceSiemensInputStruct(byte tdpuSize, ushort srcTsap, ushort dstTsap)
        {
            TdpuSize = tdpuSize;
            TsapSrc = srcTsap;
            TsapDst = dstTsap;
        }

        public byte TdpuSize;

        public ushort TsapSrc;

        public ushort TsapDst;
    }

    internal class CreateReferenceSiemensOutputStruct : OutputStruct
    {
        public CreateReferenceSiemensOutputStruct(byte tdpuSize, ushort srcTsap, ushort dstTsap)
        {
            TdpuSize = tdpuSize;
            TsapSrc = srcTsap;
            TsapDst = dstTsap;
        }

        public byte TdpuSize { get; private set; }
        public ushort TsapSrc { get; private set; }
        public ushort TsapDst { get; private set; }
    }

    internal class CreateReferenceSiemensProtocal : SpecialProtocalUnit
    {
        public override byte[] Format(InputStruct message)
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
            byte tdpuSizeContent = r_message.TdpuSize;
            const ushort srcTsapCode = 0xc102;
            ushort srcTsapContent = r_message.TsapSrc;
            const ushort dstTsapCode = 0xc202;
            ushort dstTsapContent = r_message.TsapDst;
            return Format(head, len, contentLen, typeCode, dstRef, srcRef, reserved, tdpuSizeCode, tdpuSizeContent,
                srcTsapCode, srcTsapContent, dstTsapCode, dstTsapContent);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
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

    internal class EstablishAssociationSiemensInputStruct : InputStruct
    {
        public EstablishAssociationSiemensInputStruct(ushort pduRef, ushort maxCalling, ushort maxCalled, ushort maxPdu)
        {
            PduRef = pduRef;
            MaxCalling = maxCalling;
            MaxCalled = maxCalled;
            MaxPdu = maxPdu;
        }

        public ushort PduRef { get; private set; }
        public ushort MaxCalling { get; private set; }
        public ushort MaxCalled { get; private set; }
        public ushort MaxPdu { get; private set; }
    }

    internal class EstablishAssociationSiemensOutputStruct : OutputStruct
    {
        public EstablishAssociationSiemensOutputStruct(ushort pduRef, ushort maxCalling, ushort maxCalled, ushort maxPdu)
        {
            PduRef = pduRef;
            MaxCalling = maxCalling;
            MaxCalled = maxCalled;
            MaxPdu = maxPdu;
        }

        public ushort PduRef { get; private set; }
        public ushort MaxCalling { get; private set; }
        public ushort MaxCalled { get; private set; }
        public ushort MaxPdu { get; private set; }
    }

    internal class EstablishAssociationSiemensProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (EstablishAssociationSiemensInputStruct) message;
            const byte protoId = 0x32;
            const byte rosctr = 0x01;
            const ushort redId = 0x0000;
            ushort pduRef = r_message.PduRef;
            const ushort parLg = 0x0008;
            const ushort datLg = 0x0000;
            const byte serviceId = 0xf0;
            const byte reserved = 0x00;
            ushort maxCalling = r_message.MaxCalling;
            ushort maxCalled = r_message.MaxCalled;
            ushort maxPdu = r_message.MaxPdu;
            return Format(new byte[7], protoId, rosctr, redId, pduRef, parLg, datLg, serviceId, reserved, maxCalling,
                maxCalled, maxPdu);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 4;
            ushort pduRef = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 14;
            ushort maxCalling = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            ushort maxCalled = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            ushort maxPdu = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            return new EstablishAssociationSiemensOutputStruct(pduRef,maxCalling,maxCalled,maxPdu);
        }
    }

    public class ReadRequestSiemensInputStruct : InputStruct
    {
        public ReadRequestSiemensInputStruct(ushort pduRef, SiemensTypeCode getType, string startAddress, ushort getCount, AddressTranslator addressTranslator)
        {
            PduRef = pduRef;
            TypeCode = (byte) getType;
            var address = addressTranslator.AddressTranslate(startAddress, true);
            Offset = address.Address;
            int area = address.Area;
            Area = (byte)(area%256);
            DbBlock = Area == 0x84 ? (ushort)(area/256) : (ushort)0;
            NumberOfElements = getCount;         
        }

        public ushort PduRef { get; private set; }
        public byte TypeCode { get; private set; }
        public ushort NumberOfElements { get; private set; }
        public ushort DbBlock { get; private set; }
        public byte Area { get; private set; }
        public int Offset { get; private set; }
    }
      
    public class ReadRequestSiemensOutputStruct : OutputStruct
    {
        public ReadRequestSiemensOutputStruct(ushort pduRef, SiemensAccessResult accessResult, SiemensDataType dataType, ushort getLength, byte[] value)
        {
            PduRef = pduRef;
            AccessResult = accessResult;
            DataType = dataType;
            GetLength = getLength;
            GetValue = value;
        }

        public ushort PduRef { get; private set; }
        public SiemensAccessResult AccessResult { get; private set; }
        public SiemensDataType DataType { get; private set; }
        public ushort GetLength { get; private set; }
        public byte[] GetValue { get; private set; }
    }

    public class ReadRequestSiemensProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadRequestSiemensInputStruct) message;
            const byte protoId = 0x32;
            const byte rosctr = 0x01;
            const ushort redId = 0x0000;
            ushort pduRef = r_message.PduRef;
            const ushort parLg = 14; // 参数字节数（2+12的倍数），目前仅为14
            const ushort datLg = 0; // 数据字节数
            const byte serviceId = 0x04;
            const byte numberOfVariables = 1;
            const byte variableSpec = 0x12;
            const byte vAddrLg = 0x0A;
            const byte syntaxId = 0x10;
            byte type = r_message.TypeCode;
            ushort numberOfElements = r_message.NumberOfElements;
            ushort dbBlock = r_message.DbBlock;
            byte area = r_message.Area;
            int offsetBit = r_message.Offset*8;
            byte[] offsetBitBytes = BigEndianValueHelper.Instance.GetBytes(offsetBit);
            return Format(new byte[7], protoId, rosctr, redId, pduRef, parLg, datLg, serviceId, numberOfVariables
                , variableSpec, vAddrLg, syntaxId, type, numberOfElements, dbBlock, area,
                offsetBitBytes.Skip(1).ToArray());
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 4;
            ushort pduRef = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 14;
            byte accessResult = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte dataType = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            ushort length = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            int byteLength = length/8;
            var values = new Byte[byteLength];
            Array.Copy(messageBytes, pos, values, 0, byteLength);
            return new ReadRequestSiemensOutputStruct(pduRef, (SiemensAccessResult) accessResult,
                (SiemensDataType) dataType, length, values);
        }
    }

    public class WriteRequestSiemensInputStruct : InputStruct
    {
        public WriteRequestSiemensInputStruct(ushort pduRef, string startAddress, object[] writeValue, AddressTranslator addressTranslator)
        {
            PduRef = pduRef;
            var address = addressTranslator.AddressTranslate(startAddress, true);
            Offset = address.Address;
            int area = address.Area;
            Area = (byte)(area % 256);
            DbBlock = Area == 0x84 ? (ushort)(area / 256) : (ushort)0;
            WriteValue = writeValue;
        }

        public ushort PduRef { get; private set; }
        public ushort DbBlock { get; private set; }
        public byte Area { get; private set; }
        public int Offset { get; private set; }
        public object[] WriteValue { get; private set; }
    }

    public class WriteRequestSiemensOutputStruct : OutputStruct
    {
        public WriteRequestSiemensOutputStruct(ushort pduRef, SiemensAccessResult accessResult)
        {
            PduRef = pduRef;
            AccessResult = accessResult;
        }

        public ushort PduRef { get; private set; }
        public SiemensAccessResult AccessResult {get; private set; }

    }

    public class WriteRequestSiemensProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteRequestSiemensInputStruct) message;
            byte[] valueBytes = BigEndianValueHelper.Instance.ObjectArrayToByteArray(r_message.WriteValue);
            const byte protoId = 0x32;
            const byte rosctr = 0x01;
            const ushort redId = 0x0000;
            ushort pduRef = r_message.PduRef;
            const ushort parLg = 14; // 参数字节数（2+12的倍数），目前仅为14
            ushort datLg = (ushort)(4+valueBytes.Length); // 数据字节数
            const byte serviceId = 0x05;
            const byte numberOfVariables = 1;
            const byte variableSpec = 0x12;
            const byte vAddrLg = 0x0A;
            const byte syntaxId = 0x10;
            const byte typeR = (byte)SiemensTypeCode.Byte;
            ushort numberOfElements = (ushort)valueBytes.Length;
            ushort dbBlock = r_message.DbBlock;
            byte area = r_message.Area;
            int offsetBit = r_message.Offset * 8;
            byte[] offsetBitBytes = BigEndianValueHelper.Instance.GetBytes(offsetBit);
            const byte reserved = 0x00;
            const byte type = (byte)SiemensDataType.OtherAccess;
            ushort numberOfWriteBits = (ushort)(valueBytes.Length*8);
            return Format(new byte[7], protoId, rosctr, redId, pduRef, parLg, datLg, serviceId, numberOfVariables
                , variableSpec, vAddrLg, syntaxId, typeR, numberOfElements, dbBlock, area,
                offsetBitBytes.Skip(1).ToArray(), reserved, type, numberOfWriteBits, valueBytes);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 4;
            ushort pduRef = BigEndianValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 14;
            byte accessResult = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            return new WriteRequestSiemensOutputStruct(pduRef, (SiemensAccessResult)accessResult);
        }
    }

    public class ReadTimeSiemensInputStruct : InputStruct
    {
        public ReadTimeSiemensInputStruct(ushort pduRef)
        {
            PduRef = pduRef;
        }

        public ushort PduRef { get; private set; }
    }

    public class ReadTimeSiemensOutputStruct : OutputStruct
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

    public class ReadTimeSiemensProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            throw new NotImplementedException();
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            throw new NotImplementedException();
        }
    }

    public class WriteTimeSiemensInputStruct : InputStruct
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

    public class WriteTimeSiemensOutputStruct : OutputStruct
    {
        public WriteTimeSiemensOutputStruct(ushort pduRef, byte errCod)
        {
            PduRef = pduRef;
            ErrCod = errCod;
        }

        public ushort PduRef { get; private set; }
        public byte ErrCod { get;private set; }
    }

    public class WriteTimeSiemensProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            throw new NotImplementedException();
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            throw new NotImplementedException();
        }
    }

    public class SiemensProtocalErrorException : ProtocalErrorException
    {
        public int ErrorClass { get; private set; }
        public int ErrorCode { get; private set; }
        private static readonly Dictionary<int, string> ProtocalErrorDictionary = new Dictionary<int, string>()
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
            {0xEF, "Layer 2 specific error"},
        };

        public SiemensProtocalErrorException(int errCls, int errCod)
            : base(ProtocalErrorDictionary[errCls] + " : " + errCod)
        {
            ErrorClass = errCls;
            ErrorCode = errCod;
        }
    }
}
