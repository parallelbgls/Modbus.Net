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
        public ReadRequestOpcInputStruct(string tag)
        {
            Tag = tag;
        }

        public string Tag { get; }
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
            return Format((byte) 0x00, Encoding.UTF8.GetBytes(r_message.Tag));
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
        public WriteRequestOpcInputStruct(string tag, object setValue)
        {
            Tag = tag;
            SetValue = setValue;
        }

        public string Tag { get; }
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
            return Format((byte) 0x01, tag, 0x00ffff00, fullName, 0x00ffff00, r_message.SetValue);
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