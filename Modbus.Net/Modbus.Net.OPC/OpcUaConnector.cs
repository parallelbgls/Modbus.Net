using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     OpcDa协议连接实现
    /// </summary>
    public class OpcUaConnector : OpcConnector
    {
        protected static Dictionary<string, OpcUaConnector> _instances = new Dictionary<string, OpcUaConnector>();

        protected OpcUaConnector(string host) : base(host)
        {
            Client = new MyUaClient(new Uri(ConnectionToken));
        }

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