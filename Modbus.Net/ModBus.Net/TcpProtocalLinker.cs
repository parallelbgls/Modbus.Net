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
        
        protected TcpProtocalLinker() : this(ConfigurationManager.IP)
        {
            
        }

        protected TcpProtocalLinker(string ip)
        {
            int port;
            //是否启用ConfigurationManager里的Port参数
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