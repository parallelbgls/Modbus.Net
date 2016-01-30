using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    public abstract class OpcProtocal : BaseProtocal
    {
        
    }

    public class ReadRequestOpcInputStruct : InputStruct
    {
        public ReadRequestOpcInputStruct(string tag, int getCount)
        {
            Tag = tag;
            GetCount = getCount;
        }

        public string Tag { get; private set; }
        public int GetCount { get; private set; }
    }

    public class ReadRequestOpcOutputStruct : OutputStruct
    {
        public ReadRequestOpcOutputStruct(byte[] value)
        {
            GetValue = value;
        }

        public byte[] GetValue { get; private set; }
    }

    public class ReadRequestOpcProtocal : SpecialProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadRequestOpcInputStruct) message;
            return Format(Encoding.UTF8.GetBytes(r_message.Tag));
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {          
            return new ReadRequestOpcOutputStruct(messageBytes);
        }
    }

    /*public class WriteRequestSiemensInputStruct : InputStruct
    {
        public WriteRequestSiemensInputStruct(ushort pduRef, string startAddress, object[] writeValue, AddressTranslator addressTranslator)
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

    public class WriteRequestSiemensOutputStruct : OutputStruct
    {
        public WriteRequestSiemensOutputStruct(ushort pduRef, SiemensAccessResult accessResult)
        {
            PduRef = pduRef;
            AccessResult = accessResult;
        }

        public ushort PduRef { get; private set; }
        public SiemensAccessResult AccessResult { get; private set; }

    }

    public class WriteRequestSiemensProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteRequestSiemensInputStruct)message;
            byte[] valueBytes = BigEndianValueHelper.Instance.ObjectArrayToByteArray(r_message.WriteValue);
            const byte protoId = 0x32;
            const byte rosctr = 0x01;
            const ushort redId = 0x0000;
            ushort pduRef = r_message.PduRef;
            const ushort parLg = 14; // 参数字节数（2+12的倍数），目前仅为14
            ushort datLg = (ushort)(4 + valueBytes.Length); // 数据字节数
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
            ushort numberOfWriteBits = (ushort)(valueBytes.Length * 8);
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
    }*/
}
