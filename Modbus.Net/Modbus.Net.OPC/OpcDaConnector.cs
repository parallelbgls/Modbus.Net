using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc DA连接实现
    /// </summary>
    public class OpcDaConnector : OpcConnector
    {
        /// <summary>
        ///     DA单例管理
        /// </summary>
        protected static Dictionary<string, OpcDaConnector> _instances = new Dictionary<string, OpcDaConnector>();

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc DA 服务地址</param>
        protected OpcDaConnector(string host) : base(host)
        {
            Client = new MyDaClient(new Uri(ConnectionToken));
        }

        /// <summary>
        ///     根据服务地址生成DA单例
        /// </summary>
        /// <param name="host">Opc DA 服务地址</param>
        /// <returns>Opc DA 连接器实例</returns>
        public static OpcDaConnector Instance(string host)
        {
            if (!_instances.ContainsKey(host))
            {
                var connector = new OpcDaConnector(host);
                _instances.Add(host, connector);
            }
            return _instances[host];
        }
    }
}