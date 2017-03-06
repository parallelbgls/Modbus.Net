using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     OpcUa协议
    /// </summary>
    public class OpcUaProtocal : OpcProtocal
    {
        private readonly string _host;
        private int _connectTryCount;

        public OpcUaProtocal(string host)
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
            ProtocalLinker = new OpcUaProtocalLinker(_host);
            if (!await ProtocalLinker.ConnectAsync()) return false;
            _connectTryCount = 0;
            return true;
        }
    }
}
