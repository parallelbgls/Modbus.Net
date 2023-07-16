namespace Modbus.Net.HJ212
{
    /// <summary>
    ///     HJ212协议连接器
    /// </summary>
    public class HJ212ProtocolLinker : ProtocolLinker
    {
        public HJ212ProtocolLinker(string connectionToken)
        {
            BaseConnector = new TcpConnector(connectionToken, 443);
            ((IConnectorWithController<byte[], byte[]>)BaseConnector).AddController(new FifoController(1000));
        }

        /// <summary>
        ///     检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public override bool? CheckRight(byte[] content)
        {
            return true;
        }
    }
}