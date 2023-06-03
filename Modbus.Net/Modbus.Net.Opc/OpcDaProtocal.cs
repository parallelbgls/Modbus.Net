using System.Threading.Tasks;

namespace Modbus.Net.Opc
{
    /// <summary>
    ///     Opc Da协议
    /// </summary>
    public class OpcDaProtocol : OpcProtocol
    {
        private readonly string _host;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc DA服务地址</param>
        public OpcDaProtocol(string host)
        {
            _host = host;
        }


        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override async Task<bool> ConnectAsync()
        {
            ProtocolLinker = new OpcDaProtocolLinker(_host);
            if (!await ProtocolLinker.ConnectAsync())
                return false;
            return true;
        }
    }
}