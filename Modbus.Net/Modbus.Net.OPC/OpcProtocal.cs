using System.Text;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc协议
    /// </summary>
    public abstract class OpcProtocal : BaseProtocal
    {
        protected OpcProtocal() : base(0, 0, Endian.BigEndianLsb)
        {
        }
    }

    #region 读数据

    public class ReadRequestOpcInputStruct : IInputStruct
    {
        public ReadRequestOpcInputStruct(string tag, string split)
        {
            Tag = tag;
            Split = split;
        }

        public string Tag { get; }
        public string Split { get; }
    }

    public class ReadRequestOpcOutputStruct : IOutputStruct
    {
        public ReadRequestOpcOutputStruct(byte[] value)
        {
            GetValue = value;
        }

        public byte[] GetValue { get; private set; }
    }

    public class ReadRequestOpcProtocal : ProtocalUnit, ISpecialProtocalUnit
    {
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (ReadRequestOpcInputStruct) message;
            return Format((byte) 0x00, Encoding.UTF8.GetBytes(r_message.Tag), 0x00ffff00, Encoding.UTF8.GetBytes(r_message.Split));
        }

        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            return new ReadRequestOpcOutputStruct(messageBytes);
        }
    }

    #endregion

    #region 写数据

    public class WriteRequestOpcInputStruct : IInputStruct
    {
        public WriteRequestOpcInputStruct(string tag, string split, object setValue)
        {
            Tag = tag;
            Split = split;
            SetValue = setValue;
        }

        public string Tag { get; }
        public string Split { get; }
        public object SetValue { get; }
    }

    public class WriteRequestOpcOutputStruct : IOutputStruct
    {
        public WriteRequestOpcOutputStruct(bool writeResult)
        {
            WriteResult = writeResult;
        }

        public bool WriteResult { get; private set; }
    }

    public class WriteRequestOpcProtocal : ProtocalUnit, ISpecialProtocalUnit
    {
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (WriteRequestOpcInputStruct) message;
            var tag = Encoding.UTF8.GetBytes(r_message.Tag);
            var fullName = Encoding.UTF8.GetBytes(r_message.SetValue.GetType().FullName);
            var split = Encoding.UTF8.GetBytes(r_message.Split);
            return Format((byte) 0x01, tag, 0x00ffff00, split, 0x00ffff00, fullName, 0x00ffff00, r_message.SetValue);
        }

        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            var ansByte = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
            var ans = ansByte != 0;
            return new WriteRequestOpcOutputStruct(ans);
        }
    }

    #endregion
}