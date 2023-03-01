using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc UA连接实现
    /// </summary>
    public class OpcUaConnector : OpcConnector
    {
        /// <summary>
        ///     UA单例管理
        /// </summary>
        protected static Dictionary<string, OpcUaConnector> _instances = new Dictionary<string, OpcUaConnector>();

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc UA 服务地址</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        protected OpcUaConnector(string host, bool isRegexOn) : base(host, isRegexOn)
        {
            Client = new MyUaClient(new Uri(ConnectionToken));
        }

        /// <summary>
        ///     根据地址获取UA连接器单例
        /// </summary>
        /// <param name="host">Opc UA服务地址</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        /// <returns>OPC UA实例</returns>
        public static OpcUaConnector Instance(string host, bool isRegexOn)
        {
            if (!_instances.ContainsKey(host))
            {
                var connector = new OpcUaConnector(host, isRegexOn);
                _instances.Add(host, connector);
            }
            return _instances[host];
        }
    }
}