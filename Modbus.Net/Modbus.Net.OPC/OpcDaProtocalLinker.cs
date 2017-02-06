using System.Text;

namespace Modbus.Net.OPC
{
    public class OpcDaProtocalLinker : ProtocalLinker
    {
        public OpcDaProtocalLinker() : this(ConfigurationManager.OpcDaHost)
        {
        }

        public OpcDaProtocalLinker(string host)
        {
            BaseConnector = OpcDaConnector.Instance(host);
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