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
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcUaProtocalLinker(bool isRegexOn) : this(ConfigurationManager.AppSettings["OpcUaHost"], isRegexOn)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc UA服务地址</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcUaProtocalLinker(string host, bool isRegexOn)
        {
            BaseConnector = OpcUaConnector.Instance(host, isRegexOn);
        }
    }
}