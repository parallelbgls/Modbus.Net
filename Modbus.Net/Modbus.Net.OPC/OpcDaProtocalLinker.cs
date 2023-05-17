using System.Configuration;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Da协议连接器
    /// </summary>
    public class OpcDaProtocolLinker : OpcProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcDaProtocolLinker(bool isRegexOn) : this(ConfigurationManager.AppSettings["OpcDaHost"], isRegexOn)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc DA服务地址</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcDaProtocolLinker(string host, bool isRegexOn)
        {
            BaseConnector = OpcDaConnector.Instance(host, isRegexOn);
        }
    }
}