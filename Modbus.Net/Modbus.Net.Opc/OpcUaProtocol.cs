using System.Threading.Tasks;

namespace Modbus.Net.Opc
{
    /// <summary>
    ///     Opc UA协议
    /// </summary>
    public class OpcUaProtocol : OpcProtocol
    {
        private readonly string _host;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc UA服务地址</param>
        public OpcUaProtocol(string host)
        {
            _host = host;
        }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override async Task<bool> ConnectAsync()
        {
            ProtocolLinker = new OpcUaProtocolLinker(_host);
            if (!await ProtocolLinker.ConnectAsync()) return false;
            return true;
        }
    }
}