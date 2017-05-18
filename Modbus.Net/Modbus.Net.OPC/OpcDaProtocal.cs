using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Da协议
    /// </summary>
    public class OpcDaProtocal : OpcProtocal
    {
        private readonly string _host;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc DA服务地址</param>
        public OpcDaProtocal(string host)
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
            ProtocalLinker = new OpcDaProtocalLinker(_host);
            if (!await ProtocalLinker.ConnectAsync())
                return false;
            return true;
        }
    }
}