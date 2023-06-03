namespace Modbus.Net.Opc
{
    /// <summary>
    ///     Opc Da协议连接器
    /// </summary>
    public class OpcDaProtocolLinker : OpcProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        public OpcDaProtocolLinker() : this(ConfigurationReader.GetValueDirect("OpcDa", "Host"))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc DA服务地址</param>
        public OpcDaProtocolLinker(string host)
        {
            BaseConnector = OpcDaConnector.Instance(host);
        }
    }
}