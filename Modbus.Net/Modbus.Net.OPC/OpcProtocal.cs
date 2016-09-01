using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    public abstract class OpcProtocal : BaseProtocal
    {
        protected OpcProtocal() : base(0, 0)
        {
        }
    }

    public class ReadRequestOpcInputStruct : InputStruct
    {
        public ReadRequestOpcInputStruct(string tag)
        {
            Tag = tag;
        }

        public string Tag { get; private set; }
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
            return Format((byte)0x00, Encoding.UTF8.GetBytes(r_message.Tag));
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {          
            return new ReadRequestOpcOutputStruct(messageBytes);
        }
    }

    public class WriteRequestOpcInputStruct : InputStruct
    {
        public WriteRequestOpcInputStruct(string tag, object setValue)
        {
            Tag = tag;
            SetValue = setValue;
        }

        public string Tag { get; private set; }
        public object SetValue { get; private set; }
    }

    public class WriteRequestOpcOutputStruct : OutputStruct
    {
        public WriteRequestOpcOutputStruct(bool writeResult)
        {
            WriteResult = writeResult;
        }

        public bool WriteResult { get; private set; }

    }

    public class WriteRequestOpcProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteRequestOpcInputStruct)message;
            byte[] tag = Encoding.UTF8.GetBytes(r_message.Tag);
            return Format((byte)0x00, tag, (int)0x00ffff00, r_message.SetValue.GetType().FullName, (int)0x00ffff00, r_message.SetValue);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            var ansByte = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            var ans = ansByte != 0;
            return new WriteRequestOpcOutputStruct(ans);
        }
    }
}
