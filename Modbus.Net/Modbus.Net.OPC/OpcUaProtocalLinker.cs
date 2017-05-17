using System.Configuration;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Da协议连接器
    /// </summary>
    public class OpcUaProtocalLinker : OpcProtocalLinker
    {
        public OpcUaProtocalLinker() : this(ConfigurationManager.AppSettings["OpcUaHost"])
        {
        }

        public OpcUaProtocalLinker(string host)
        {
            BaseConnector = OpcUaConnector.Instance(host);
        }
    }
}