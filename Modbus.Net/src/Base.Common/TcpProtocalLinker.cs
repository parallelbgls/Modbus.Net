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
        protected TcpProtocalLinker() : this(ConfigurationManager.IP, int.Parse(ConfigurationManager.ModbusPort))
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        protected TcpProtocalLinker(string ip, int port)
        {
            //初始化连接对象
            BaseConnector = new TcpConnector(ip, port, int.Parse(ConfigurationManager.IPConnectionTimeout));
        }
    }
}