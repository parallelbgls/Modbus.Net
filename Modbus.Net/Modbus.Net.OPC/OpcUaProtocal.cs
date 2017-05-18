using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc UA协议
    /// </summary>
    public class OpcUaProtocal : OpcProtocal
    {
        private readonly string _host;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc UA服务地址</param>
        public OpcUaProtocal(string host)
        {
            _host = host;
        }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override bool Connect()
        {
            return AsyncHelper.RunSync(ConnectAsync);
        }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override async Task<bool> ConnectAsync()
        {
            ProtocalLinker = new OpcUaProtocalLinker(_host);
            if (!await ProtocalLinker.ConnectAsync()) return false;
            return true;
        }
    }
}