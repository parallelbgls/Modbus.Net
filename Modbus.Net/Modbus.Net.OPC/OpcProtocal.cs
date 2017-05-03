using System.Text;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc协议
    /// </summary>
    public abstract class OpcProtocal : BaseProtocal<OpcParamIn, OpcParamOut, ProtocalUnit<OpcParamIn, OpcParamOut>>
    {
        protected OpcProtocal() : base(0, 0, Endian.BigEndianLsb)
        {
        }
    }

    #region 读数据

    public class ReadRequestOpcInputStruct : IInputStruct
    {
        public ReadRequestOpcInputStruct(string tag, char split)
        {
            Tag = tag;
            Split = split;
        }

        public string Tag { get; }
        public char Split { get; }
    }

    public class ReadRequestOpcOutputStruct : IOutputStruct
    {
        public ReadRequestOpcOutputStruct(byte[] value)
        {
            GetValue = value;
        }

        public byte[] GetValue { get; private set; }
    }

    public class ReadRequestOpcProtocal : ProtocalUnit<OpcParamIn, OpcParamOut>, ISpecialProtocalUnit
    {
        public override OpcParamIn Format(IInputStruct message)
        {
            var r_message = (ReadRequestOpcInputStruct) message;
            return new OpcParamIn()
            {
                IsRead = true,
                Tag = r_message.Tag,
                Split = r_message.Split
            };
        }

        public override IOutputStruct Unformat(OpcParamOut messageBytes, ref int pos)
        {
            return new ReadRequestOpcOutputStruct(messageBytes.Value);
        }
    }

    #endregion

    #region 写数据

    public class WriteRequestOpcInputStruct : IInputStruct
    {
        public WriteRequestOpcInputStruct(string tag, char split, object setValue)
        {
            Tag = tag;
            Split = split;
            SetValue = setValue;
        }

        public string Tag { get; }
        public char Split { get; }
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

    public class WriteRequestOpcProtocal : ProtocalUnit<OpcParamIn, OpcParamOut>, ISpecialProtocalUnit
    {
        public override OpcParamIn Format(IInputStruct message)
        {
            var r_message = (WriteRequestOpcInputStruct) message;
            return new OpcParamIn()
            {
                IsRead = false,
                Tag = r_message.Tag,
                Split = r_message.Split,
                SetValue = r_message.SetValue
            };
        }

        public override IOutputStruct Unformat(OpcParamOut messageBytes, ref int pos)
        {
            var ansByte = BigEndianValueHelper.Instance.GetByte(messageBytes.Value, ref pos);
            var ans = ansByte != 0;
            return new WriteRequestOpcOutputStruct(ans);
        }
    }

    #endregion
}