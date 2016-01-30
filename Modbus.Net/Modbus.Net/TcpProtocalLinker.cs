using System;

namespace Modbus.Net
{
    /// <summary>
    /// Tcp连接对象
    /// </summary>
    public abstract class TcpProtocalLinker : ProtocalLinker
    {
        
        protected TcpProtocalLinker() : this(ConfigurationManager.IP, int.Parse(ConfigurationManager.ModbusPort))
        {
            
        }

        protected TcpProtocalLinker(string ip, int port)
        {
            _baseConnector = new TcpConnector(ip, port, int.Parse(ConfigurationManager.IPConnectionTimeout));
        }
    }   
}