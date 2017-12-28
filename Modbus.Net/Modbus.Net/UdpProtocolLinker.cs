using System.Configuration;

namespace Modbus.Net
{
    /// <summary>
    ///     Udp连接对象
    /// </summary>
    public abstract class UdpProtocolLinker : ProtocolLinker
    {
        /// <summary>
        ///     构造器
        /// </summary>
        protected UdpProtocolLinker(int port)
            : this(ConfigurationManager.AppSettings["IP"], port)
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        protected UdpProtocolLinker(string ip, int port)
            : this(ip, port, int.Parse(ConfigurationManager.AppSettings["IPConnectionTimeout"] ?? "-1"))
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="connectionTimeout">超时时间</param>
        protected UdpProtocolLinker(string ip, int port, int connectionTimeout)
        {
            if (connectionTimeout == -1)
            {
                //初始化连接对象
                BaseConnector = new UdpConnector(ip, port);
            }
            else
            {
                //初始化连接对象
                BaseConnector = new UdpConnector(ip, port, connectionTimeout);
            }
        }
    }
}