using System.Configuration;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc UA协议连接器
    /// </summary>
    public class OpcUaProtocalLinker : OpcProtocalLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        public OpcUaProtocalLinker() : this(ConfigurationManager.AppSettings["OpcUaHost"])
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc UA服务地址</param>
        public OpcUaProtocalLinker(string host)
        {
            BaseConnector = OpcUaConnector.Instance(host);
        }
    }
}