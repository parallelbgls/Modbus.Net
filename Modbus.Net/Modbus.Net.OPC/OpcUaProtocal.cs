using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc UA协议
    /// </summary>
    public class OpcUaProtocal : OpcProtocal
    {
        private readonly string _host;

        private readonly bool _isRegexOn;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc UA服务地址</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcUaProtocal(string host, bool isRegexOn)
        {
            _host = host;
            _isRegexOn = isRegexOn;
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
            ProtocalLinker = new OpcUaProtocalLinker(_host, _isRegexOn);
            if (!await ProtocalLinker.ConnectAsync()) return false;
            return true;
        }
    }
}