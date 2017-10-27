using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Da协议
    /// </summary>
    public class OpcDaProtocal : OpcProtocal
    {
        private readonly string _host;

        private readonly bool _isRegexOn;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">Opc DA服务地址</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcDaProtocal(string host, bool isRegexOn)
        {
            _host = host;
            _isRegexOn = isRegexOn;
        }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override async Task<bool> ConnectAsync()
        {
            ProtocalLinker = new OpcDaProtocalLinker(_host, _isRegexOn);
            if (!await ProtocalLinker.ConnectAsync())
                return false;
            return true;
        }
    }
}