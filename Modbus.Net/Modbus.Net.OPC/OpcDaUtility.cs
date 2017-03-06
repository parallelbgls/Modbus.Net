using System;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Da协议Api入口
    /// </summary>
    public class OpcDaUtility : OpcUtility
    {
        public OpcDaUtility(string connectionString) : base(connectionString)
        {
            Wrapper = new OpcDaProtocal(ConnectionString);
        }
    }
}