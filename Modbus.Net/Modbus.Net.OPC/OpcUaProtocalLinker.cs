using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    /// Opc Da协议连接器
    /// </summary>
    public class OpcUaProtocalLinker : ProtocalLinker
    {
        public OpcUaProtocalLinker() : this(ConfigurationManager.OpcUaHost)
        {
        }

        public OpcUaProtocalLinker(string host)
        {
            BaseConnector = OpcUaConnector.Instance(host);
        }

        public override bool? CheckRight(byte[] content)
        {
            if (content != null && content.Length == 6 && Encoding.ASCII.GetString(content) == "NoData")
            {
                return null;
            }
            return base.CheckRight(content);
        }
    }
}
