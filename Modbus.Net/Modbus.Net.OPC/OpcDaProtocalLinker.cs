using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.OPC
{
    public class OpcDaProtocalLinker : ProtocalLinker
    {
        public OpcDaProtocalLinker() : this(ConfigurationManager.OpcDaHost)
        {

        }

        public OpcDaProtocalLinker(string host)
        {
            _baseConnector = OpcDaConnector.Instance(host);
        }

        public override bool CheckRight(byte[] content)
        {
            if (content != null && content.Length == 6 && Encoding.ASCII.GetString(content) == "NoData")
            {
                return false;
            }
            return base.CheckRight(content);
        }
    }
}
