using System;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    public abstract class TcpProtocalLinker : ProtocalLinker
    {
        /// <summary>
        /// 连接对象
        /// </summary>
        
        protected TcpProtocalLinker() : this(ConfigurationManager.IP)
        {
            
        }

        protected TcpProtocalLinker(string ip)
        {
            int port;
            if (ConfigurationManager.ResourceManager.GetString("Port") != null && int.TryParse(ConfigurationManager.ResourceManager.GetString("Port"),out port))
            {

            }
            else
            {
                port = 502;
            }
            _baseConnector = new TcpConnector(ip, port, false);
        }
    }   
}