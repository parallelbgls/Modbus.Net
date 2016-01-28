using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.OPC
{
    public class OpcDaProtocal : OpcProtocal
    {
        private int _connectTryCount;

        private readonly string _host;

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
