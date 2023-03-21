namespace Modbus.Net
{
    /// <summary>
    ///     Tcp连接对象
    /// </summary>
    public abstract class TcpProtocolLinker : ProtocolLinker
    {
        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="connectionTimeout">超时时间</param>
        /// <param name="isFullDuplex">是否为全双工</param>
        protected TcpProtocolLinker(string ip, int port, int? connectionTimeout = null, bool? isFullDuplex = null)
        {
            connectionTimeout = int.Parse(connectionTimeout != null ? connectionTimeout.ToString() : null ?? ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "ConnectionTimeout"));
            isFullDuplex = bool.Parse(isFullDuplex != null ? isFullDuplex.ToString() : null ?? ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "FullDuplex"));
            //初始化连接对象
            BaseConnector = new TcpConnector(ip, port, connectionTimeout.Value, isFullDuplex.Value);
        }
    }
}