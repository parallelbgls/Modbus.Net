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
        
        protected TcpProtocalLinker()
        {
            //初始化连对象
            _baseConnector = new TcpConnector(ConfigurationManager.IP, int.Parse(ConfigurationManager.Port), false);
        }

        protected TcpProtocalLinker(string ip)
        {
            _baseConnector = new TcpConnector(ip, int.Parse(ConfigurationManager.Port), false);
        }
    }   
}