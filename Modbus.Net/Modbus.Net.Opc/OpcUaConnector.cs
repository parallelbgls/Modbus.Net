using System;
using System.Collections.Generic;

namespace Modbus.Net.Opc
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
        protected OpcUaConnector(string host) : base(host)
        {
            Client = new MyUaClient(new Uri(ConnectionToken));
        }

        /// <summary>
        ///     根据地址获取UA连接器单例
        /// </summary>
        /// <param name="host">Opc UA服务地址</param>
        /// <returns>Opc UA实例</returns>
        public static OpcUaConnector Instance(string host)
        {
            if (!_instances.ContainsKey(host))
            {
                var connector = new OpcUaConnector(host);
                _instances.Add(host, connector);
            }
            return _instances[host];
        }
    }
}