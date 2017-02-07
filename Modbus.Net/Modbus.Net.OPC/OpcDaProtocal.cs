using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     OpcDa协议
    /// </summary>
    public class OpcDaProtocal : OpcProtocal
    {
        private readonly string _host;
        private int _connectTryCount;

        public OpcDaProtocal(string host)
        {
            _host = host;
        }

        public override bool Connect()
        {
            return AsyncHelper.RunSync(ConnectAsync);
        }

        public override async Task<bool> ConnectAsync()
        {
            _connectTryCount++;
            ProtocalLinker = new OpcDaProtocalLinker(_host);
            if (!await ProtocalLinker.ConnectAsync()) return false;
            _connectTryCount = 0;
            return true;
        }
    }
}