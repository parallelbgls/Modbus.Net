namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc协议
    /// </summary>
    public abstract class OpcProtocol : BaseProtocol<OpcParamIn, OpcParamOut, ProtocolUnit<OpcParamIn, OpcParamOut>,
        PipeUnit<OpcParamIn, OpcParamOut, IProtocolLinker<OpcParamIn, OpcParamOut>,
            ProtocolUnit<OpcParamIn, OpcParamOut>>>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        protected OpcProtocol() : base(0, 0, Endian.BigEndianLsb)
        {
        }
    }

    #region 读数据

    /// <summary>
    ///     读数据输入
    /// </summary>
    public class ReadRequestOpcInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="split">分隔符</param>
        public ReadRequestOpcInputStruct(string[] tag, char split)
        {
            Tag = tag;
            Split = split;
        }

        /// <summary>
        ///     标签
        /// </summary>
        public string[] Tag { get; }

        /// <summary>
        ///     分隔符
        /// </summary>
        public char Split { get; }
    }

    /// <summary>
    ///     读地址输出
    /// </summary>
    public class ReadRequestOpcOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="value">读取的数据</param>
        public ReadRequestOpcOutputStruct(byte[] value)
        {
            GetValue = value;
        }

        /// <summary>
        ///     读取的地址
        /// </summary>
        public byte[] GetValue { get; private set; }
    }

    /// <summary>
    ///     读数据协议
    /// </summary>
    [SpecialProtocolUnit]
    public class ReadRequestOpcProtocol : ProtocolUnit<OpcParamIn, OpcParamOut>
    {
        /// <summary>
        ///     从对象的参数数组格式化
        /// </summary>
        /// <param name="message">非结构化的输入数据</param>
        /// <returns>格式化后的字节流</returns>
        public override OpcParamIn Format(IInputStruct message)
        {
            var r_message = (ReadRequestOpcInputStruct) message;
            return new OpcParamIn
            {
                IsRead = true,
                Tag = r_message.Tag,
                Split = r_message.Split
            };
        }

        /// <summary>
        ///     把仪器返回的内容填充到输出结构中
        /// </summary>
        /// <param name="messageBytes">返回数据的字节流</param>
        /// <param name="pos">转换标记位</param>
        /// <returns>结构化的输出数据</returns>
        public override IOutputStruct Unformat(OpcParamOut messageBytes, ref int pos)
        {
            return new ReadRequestOpcOutputStruct(messageBytes.Value);
        }
    }

    #endregion

    #region 写数据

    /// <summary>
    ///     写数据输入
    /// </summary>
    public class WriteRequestOpcInputStruct : IInputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="split">分隔符</param>
        /// <param name="setValue">写入的数据</param>
        public WriteRequestOpcInputStruct(string[] tag, char split, object setValue)
        {
            Tag = tag;
            Split = split;
            SetValue = setValue;
        }

        /// <summary>
        ///     标签
        /// </summary>
        public string[] Tag { get; }

        /// <summary>
        ///     分隔符
        /// </summary>
        public char Split { get; }

        /// <summary>
        ///     写入的数据
        /// </summary>
        public object SetValue { get; }
    }

    /// <summary>
    ///     写数据输出
    /// </summary>
    public class WriteRequestOpcOutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="writeResult">写入是否成功</param>
        public WriteRequestOpcOutputStruct(bool writeResult)
        {
            WriteResult = writeResult;
        }

        /// <summary>
        ///     写入是否成功
        /// </summary>
        public bool WriteResult { get; private set; }
    }

    /// <summary>
    ///     写数据协议
    /// </summary>
    [SpecialProtocolUnit]
    public class WriteRequestOpcProtocol : ProtocolUnit<OpcParamIn, OpcParamOut>
    {
        /// <summary>
        ///     从对象的参数数组格式化
        /// </summary>
        /// <param name="message">非结构化的输入数据</param>
        /// <returns>格式化后的字节流</returns>
        public override OpcParamIn Format(IInputStruct message)
        {
            var r_message = (WriteRequestOpcInputStruct) message;
            return new OpcParamIn
            {
                IsRead = false,
                Tag = r_message.Tag,
                Split = r_message.Split,
                SetValue = r_message.SetValue
            };
        }

        /// <summary>
        ///     把仪器返回的内容填充到输出结构中
        /// </summary>
        /// <param name="messageBytes">返回数据的字节流</param>
        /// <param name="pos">转换标记位</param>
        /// <returns>结构化的输出数据</returns>
        public override IOutputStruct Unformat(OpcParamOut messageBytes, ref int pos)
        {
            var ansByte = BigEndianValueHelper.Instance.GetByte(messageBytes.Value, ref pos);
            var ans = ansByte != 0;
            return new WriteRequestOpcOutputStruct(ans);
        }
    }

    #endregion
}