using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Forms;

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
        public ReadTimeSimenseOutputStruct(ushort pduRef, DateTime dateTime)
        {
            PduRef = pduRef;
            DateTime = dateTime;
        }

        public ushort PduRef { get; private set; }
        public DateTime DateTime { get; private set; }
    }

    public class ReadTimeSimenseProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadTimeSimenseInputStruct) message;
            const byte protoId = 0x32;
            const byte rosctr = 0x07;
            const ushort redId = 0x0000;
            ushort pduRef = r_message.PduRef;
            const ushort parLg = 8;
            const ushort datLg = 4;
            const byte serviceId = 0x00;
            const byte noVar = 0x01;
            const byte varSpc = 0x12;
            const byte vAddrLg = 0x04;
            const byte synId = 0x11;
            const byte classP = 0x47;
            const byte id1 = 0x01;
            const byte id2 = 0x00;
            const byte accRslt = 0x0A;
            const byte dType = 0x00;
            const ushort length = 0x0000;
            return Format(new Byte[7], protoId, rosctr, redId, pduRef, parLg, datLg, serviceId, noVar, varSpc, vAddrLg, synId, classP,
                id1, id2, accRslt, dType, length);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 4;
            ushort pduRef = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 28;
            byte year1 = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte month1 = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte day1 = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte hour1 = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte minute1 = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte second1 = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte second1_10_100 = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            byte second1_1000_weekday = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            int year = year1/16*10 + year1%16;
            int month = month1/16*10 + month1%16;
            int day = day1/16*10 + day1%16;
            int hour = hour1/16*10 + hour1%16;
            int minute = minute1/16*10 + minute1%16;
            int second = second1/16*10 + second1%16;
            int millisecond = second1_10_100 / 16 * 100 + second1_10_100 % 16 * 10 + second1_1000_weekday / 16;
            int weekday = second1_1000_weekday%16;
            DateTime dateTime = new DateTime(DateTime.Now.Year/100*100 + year, month, day, hour, minute, second, millisecond);
            if (dateTime > DateTime.Now.AddDays(1)) dateTime = dateTime.AddYears(-100);
            if (weekday == 0) return new ReadTimeSimenseOutputStruct(pduRef, dateTime); 
            while (dateTime.DayOfWeek != (DayOfWeek) (weekday - 1)) dateTime = dateTime.AddYears(-100);
            return new ReadTimeSimenseOutputStruct(pduRef, dateTime);
        }
    }

    public class WriteTimeSimenseInputStruct : InputStruct
    {
        public WriteTimeSimenseInputStruct(ushort pduRef, DateTime dateTime)
        {
            PduRef = pduRef;
            DateTime = dateTime;
        }

        public ushort PduRef { get; private set; }
        public DateTime DateTime { get; private set; }
    }

    public class WriteTimeSimenseOutputStruct : OutputStruct
    {
        public WriteTimeSimenseOutputStruct(ushort pduRef, byte id2)
        {
            PduRef = pduRef;
            Id2 = id2;
        }

        public ushort PduRef { get; private set; }

        public byte Id2 { get; private set; }
    }

    public class WriteTimeSimenseProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteTimeSimenseInputStruct) message;
            const byte protoId = 0x32;
            const byte rosctr = 0x07;
            const ushort redId = 0x0000;
            ushort pduRef = r_message.PduRef;
            const ushort parLg = 0x0008;
            const ushort datLg = 0x000e;
            const byte serviceId = 0x00;
            const byte noVar = 0x01;
            const byte varSpc = 0x12;
            const byte vAddrLg = 0x04;
            const byte synId = 0x11;
            const byte classP = 0x47;
            const byte id1 = 0x02;
            const byte id2 = 0x00;
            const byte accRslt = 0xFF;
            const byte dType = 0x09;
            const ushort length = 0x000A;
            const ushort todClockStatus = 0x0018;
            byte year = (byte) (r_message.DateTime.Year%100/10*16 + r_message.DateTime.Year%10);
            byte month = (byte) (r_message.DateTime.Month/10*16 + r_message.DateTime.Month%10);
            byte day = (byte) (r_message.DateTime.Day/10*16 + r_message.DateTime.Day%10);
            byte hour = (byte) (r_message.DateTime.Hour/10*16 + r_message.DateTime.Hour%10);
            byte minute = (byte) (r_message.DateTime.Minute/10*16 + r_message.DateTime.Minute%10);
            byte second = (byte) (r_message.DateTime.Second/10*16 + r_message.DateTime.Second%10);
            byte dayOfWeek = (byte) (r_message.DateTime.DayOfWeek + 1);
            return Format(new byte[7], protoId, rosctr, redId, pduRef, parLg, datLg, serviceId, noVar, varSpc, vAddrLg,
                synId, classP, id1, id2, accRslt, dType, length, todClockStatus, year, month, day, hour, minute, second,
                (byte)0, dayOfWeek);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            pos = 4;
            ushort pduRef = ValueHelper.Instance.GetUShort(messageBytes, ref pos);
            pos = 17;
            byte id2 = ValueHelper.Instance.GetByte(messageBytes, ref pos);
            return new WriteTimeSimenseOutputStruct(pduRef, id2);
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
