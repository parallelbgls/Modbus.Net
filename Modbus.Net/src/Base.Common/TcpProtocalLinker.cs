#if NET40||NET45||NET451||NET452||NET46||NET461||NET462||NET47
using System.Configuration;
#endif

namespace Modbus.Net
{
    /// <summary>
    ///     Tcp连接对象
    /// </summary>
    public abstract class TcpProtocalLinker : ProtocalLinker
    {
        /// <summary>
        ///     构造器
        /// </summary>
        protected TcpProtocalLinker(int port)
            : this(ConfigurationManager.AppSettings["IP"], port)
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        protected TcpProtocalLinker(string ip, int port)
            : this(ip, port, int.Parse(ConfigurationManager.AppSettings["IPConnectionTimeout"] ?? "5000"))
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="connectionTimeout">超时时间</param>
        protected TcpProtocalLinker(string ip, int port, int connectionTimeout)
        {
            //初始化连接对象
            BaseConnector = new TcpConnector(ip, port, connectionTimeout);
        }
    }
}