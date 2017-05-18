using System.Configuration;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Da协议连接器
    /// </summary>
    public class OpcDaProtocalLinker : OpcProtocalLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        public OpcDaProtocalLinker() : this(ConfigurationManager.AppSettings["OpcDaHost"])
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc DA服务地址</param>
        public OpcDaProtocalLinker(string host)
        {
            BaseConnector = OpcDaConnector.Instance(host);
        }
    }
}