using System;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    /// <summary>
    /// Tcp连接对象
    /// </summary>
    public abstract class TcpProtocalLinker : ProtocalLinker
    {
        
        protected TcpProtocalLinker() : this(ConfigurationManager.IP, int.Parse(ConfigurationManager.Port))
        {
            
        }

        protected TcpProtocalLinker(string ip, int port)
        {
            _baseConnector = new TcpConnector(ip, port, false);
        }
    }   
}