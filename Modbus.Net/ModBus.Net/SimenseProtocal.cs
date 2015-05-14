using System;
using System.Collections.Generic;
using System.Linq;

public enum SimenseTypeCode : byte
{
    Bool = 0x01,
    Byte = 0x02,
    Word = 0x03,
    DWord = 0x04,
    C = 0x1E,
    T = 0x1F,
    HC = 0x20,
};

public enum SimenseAccessResult : byte
{
    NoError = 0xFF,
    HardwareFault = 0x01,
    IllegalObjectAccess = 0x03,
    InvalidAddress = 0x05,
    DataTypeNotSupport = 0x06,
    ObjNotExistOrLengthError = 0x0A,
};

public enum SimenseDataType : byte
{
    Error = 0x00,
    BitAccess = 0x03,
    OtherAccess = 0x04
};

namespace ModBus.Net
{
    public abstract class SimenseProtocal : BaseProtocal
    {

    }

    internal class CreateReferenceSimenseInputStruct : InputStruct
    {
        public CreateReferenceSimenseInputStruct(byte tdpuSize, ushort srcTsap, ushort dstTsap)
        {
            TdpuSize = tdpuSize;
            TsapSrc = srcTsap;
            TsapDst = dstTsap;
        }

        public byte TdpuSize;

        public ushort TsapSrc;

        public ushort TsapDst;
    }

    internal class CreateReferenceSimenseOutputStruct : OutputStruct
    {
        public CreateReferenceSimenseOutputStruct(byte tdpuSize, ushort srcTsap, ushort dstTsap)
        {
            TdpuSize = tdpuSize;
            TsapSrc = srcTsap;
            TsapDst = dstTsap;
        }

        public byte TdpuSize { get; private set; }
        public ushort TsapSrc { get; private set; }
        public ushort TsapDst { get; private set; }
    }

    internal class CreateReferenceSimenseProtocal : SpecialProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (CreateReferenceSimenseInputStruct)message;
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
                    tdpuSize = ValueHelper.Instance.GetByte(messageBytes, ref pos);
                    break;
                }
                case 0xc1:
                {
                    pos += 2;
                    srcTsap = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
                    break;
                }
                case 0xc2:
                {
                    pos += 2;
                    dstTsap = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
                    break;
                }
            }
            return new CreateReferenceSimenseOutputStruct(tdpuSize, srcTsap, dstTsap);
        }
    }

    internal class EstablishAssociationSimenseInputStruct : InputStruct
    {
        public EstablishAssociationSimenseInputStruct(ushort pduRef, ushort maxCalling, ushort maxCalled, ushort maxPdu)
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

    internal class EstablishAssociationSimenseOutputStruct : OutputStruct
    {
        public EstablishAssociationSimenseOutputStruct(ushort pduRef, ushort maxCalling, ushort maxCalled, ushort maxPdu)
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

    internal class EstablishAssociationSimenseProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (EstablishAssociationSimenseInputStruct) message;
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
            ushort pduRef = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 14;
            ushort maxCalling = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
            ushort maxCalled = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
            ushort maxPdu = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
            return new EstablishAssociationSimenseOutputStruct(pduRef,maxCalling,maxCalled,maxPdu);
        }
    }

    public class ReadRequestSimenseInputStruct : InputStruct
    {
        public ReadRequestSimenseInputStruct(ushort pduRef, SimenseTypeCode getType, string startAddress, ushort getCount, AddressTranslator addressTranslator)
        {
            PduRef = pduRef;
            TypeCode = (byte) getType;
            var address = addressTranslator.AddressTranslate(startAddress, true);
            Offset = address.Key;
            int area = address.Value;
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
      
    public class ReadRequestSimenseOutputStruct : OutputStruct
    {
        public ReadRequestSimenseOutputStruct(ushort pduRef, SimenseAccessResult accessResult, SimenseDataType dataType, ushort getLength, byte[] value)
        {
            PduRef = pduRef;
            AccessResult = accessResult;
            DataType = dataType;
            GetLength = getLength;
            GetValue = value;
        }

        public ushort PduRef { get; private set; }
        public SimenseAccessResult AccessResult { get; private set; }
        public SimenseDataType DataType { get; private set; }
        public ushort GetLength { get; private set; }
        public byte[] GetValue { get; private set; }
    }

    public class ReadRequestSimenseProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadRequestSimenseInputStruct) message;
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
            byte[] offsetBitBytes = ValueHelper.Instance.GetBytes(offsetBit);
            return Format(new byte[7], protoId, rosctr, redId, pduRef, parLg, datLg, serviceId, numberOfVariables
                , variableSpec, vAddrLg, syntaxId, type, numberOfElements, dbBlock, area,
                offsetBitBytes.Skip(1).ToArray());
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 4;
            ushort pduRef = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 14;
            byte accessResult = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte dataType = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            ushort length = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
            int byteLength = length/8;
            var values = new Byte[byteLength];
            Array.Copy(messageBytes, pos, values, 0, byteLength);
            return new ReadRequestSimenseOutputStruct(pduRef, (SimenseAccessResult) accessResult,
                (SimenseDataType) dataType, length, values);
        }
    }

    public class WriteRequestSimenseInputStruct : InputStruct
    {
        public WriteRequestSimenseInputStruct(ushort pduRef, string startAddress, object[] writeValue, AddressTranslator addressTranslator)
        {
            PduRef = pduRef;
            var address = addressTranslator.AddressTranslate(startAddress, true);
            Offset = address.Key;
            int area = address.Value;
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

    public class WriteRequestSimenseOutputStruct : OutputStruct
    {
        public WriteRequestSimenseOutputStruct(ushort pduRef, SimenseAccessResult accessResult)
        {
            PduRef = pduRef;
            AccessResult = accessResult;
        }

        public ushort PduRef { get; private set; }
        public SimenseAccessResult AccessResult {get; private set; }

    }

    public class WriteRequestSimenseProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteRequestSimenseInputStruct) message;
            byte[] valueBytes = ValueHelper.Instance.ObjectArrayToByteArray(r_message.WriteValue);
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
            const byte typeR = (byte)SimenseTypeCode.Byte;
            ushort numberOfElements = (ushort)valueBytes.Length;
            ushort dbBlock = r_message.DbBlock;
            byte area = r_message.Area;
            int offsetBit = r_message.Offset * 8;
            byte[] offsetBitBytes = ValueHelper.Instance.GetBytes(offsetBit);
            const byte reserved = 0x00;
            const byte type = (byte)SimenseDataType.OtherAccess;
            ushort numberOfWriteBits = (ushort)(valueBytes.Length*8);
            return Format(new byte[7], protoId, rosctr, redId, pduRef, parLg, datLg, serviceId, numberOfVariables
                , variableSpec, vAddrLg, syntaxId, typeR, numberOfElements, dbBlock, area,
                offsetBitBytes.Skip(1).ToArray(), reserved, type, numberOfWriteBits, valueBytes);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 4;
            ushort pduRef = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 14;
            byte accessResult = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            return new WriteRequestSimenseOutputStruct(pduRef, (SimenseAccessResult)accessResult);
        }
    }

    public class ReadTimeSimenseInputStruct : InputStruct
    {
        public ReadTimeSimenseInputStruct(ushort pduRef)
        {
            PduRef = pduRef;
        }

        public ushort PduRef { get; private set; }
    }

    public class ReadTimeSimenseOutputStruct : OutputStruct
    {
        public ReadTimeSimenseOutputStruct(ushort pduRef, DateTime dateTime, TodClockStatus todClockStatus)
        {
            PduRef = pduRef;
            DateTime = dateTime;
            TodClockStatus = todClockStatus;
        }

        public ushort PduRef { get; private set; }
        public DateTime DateTime { get; private set; }
        public TodClockStatus TodClockStatus { get; private set; }
    }

    public class ReadTimeSimenseProtocal : ProtocalUnit
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

    public class WriteTimeSimenseInputStruct : InputStruct
    {
        public WriteTimeSimenseInputStruct(ushort pduRef, DateTime dateTime, TodClockStatus todClockStatus)
        {
            PduRef = pduRef;
            DateTime = dateTime;
            TodClockStatus = todClockStatus;
        }

        public ushort PduRef { get; private set; }
        public DateTime DateTime { get; private set; }
        public TodClockStatus TodClockStatus { get; private set; }
    }

    public class WriteTimeSimenseOutputStruct : OutputStruct
    {
        public WriteTimeSimenseOutputStruct(ushort pduRef, byte errCod)
        {
            PduRef = pduRef;
            ErrCod = errCod;
        }

        public ushort PduRef { get; private set; }
        public byte ErrCod { get;private set; }
    }

    public class WriteTimeSimenseProtocal : ProtocalUnit
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

    public class SimenseProtocalErrorException : ProtocalErrorException
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

        public SimenseProtocalErrorException(int errCls, int errCod)
            : base(ProtocalErrorDictionary[errCls] + " : " + errCod)
        {
            ErrorClass = errCls;
            ErrorCode = errCod;
        }
    }
}
