namespace Modbus.Net.HJ212
{
    /// <summary>
    ///     HJ212协议连接器
    /// </summary>
    public class HJ212ProtocolLinker : TcpProtocolLinker
    {
        public HJ212ProtocolLinker(string ip, int port) : base(ip, port)
        {
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