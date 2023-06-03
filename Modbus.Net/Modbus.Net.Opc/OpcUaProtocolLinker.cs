namespace Modbus.Net.Opc
{
    /// <summary>
    ///     Opc UA协议连接器
    /// </summary>
    public class OpcUaProtocolLinker : OpcProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        public OpcUaProtocolLinker() : this(ConfigurationReader.GetValueDirect("OpcUa", "Host"))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc UA服务地址</param>
        public OpcUaProtocolLinker(string host)
        {
            BaseConnector = OpcUaConnector.Instance(host);
        }
    }
}