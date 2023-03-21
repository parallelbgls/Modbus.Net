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
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="connectionTimeout">超时时间</param>
        /// <param name="isFullDuplex">是否为全双工</param>
        protected UdpProtocolLinker(string ip, int port, int? connectionTimeout = null, bool? isFullDuplex = null)
        {
            connectionTimeout = int.Parse(connectionTimeout != null ? connectionTimeout.ToString() : null ?? ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "ConnectionTimeout"));
            isFullDuplex = bool.Parse(isFullDuplex != null ? isFullDuplex.ToString() : null ?? ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "FullDuplex"));
            //初始化连接对象
            BaseConnector = new UdpConnector(ip, port, connectionTimeout.Value, isFullDuplex.Value);
        }
    }
}