using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     OpcDa协议连接实现
    /// </summary>
    public class OpcDaConnector : OpcConnector
    {
        protected static Dictionary<string, OpcDaConnector> _instances = new Dictionary<string, OpcDaConnector>();

        protected OpcDaConnector(string host) : base(host)
        {
            Client = new MyDaClient(new Uri(ConnectionToken));
        }

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