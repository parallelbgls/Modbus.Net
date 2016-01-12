using System.Threading.Tasks;

namespace ModBus.Net.Siemens
{
    public class SiemensTcpProtocal : SiemensProtocal
    {
        private ushort _taspSrc;
        private ushort _tsapDst;
        private ushort _maxCalling;
        private ushort _maxCalled;
        private ushort _maxPdu;
        private byte _tdpuSize;

        private string _ip;
        private int connectTryCount;

        public SiemensTcpProtocal(byte tdpuSize, ushort tsapSrc, ushort tsapDst, ushort maxCalling, ushort maxCalled, ushort maxPdu) : this(tdpuSize, tsapSrc, tsapDst, maxCalling, maxCalled, maxPdu, ConfigurationManager.IP)
        {
        }

        public SiemensTcpProtocal(byte tdpuSize, ushort tsapSrc, ushort tsapDst, ushort maxCalling, ushort maxCalled, ushort maxPdu, string ip)
        {
            _taspSrc = tsapSrc;
            _tsapDst = tsapDst;
            _maxCalling = maxCalling;
            _maxCalled = maxCalled;
            _maxPdu = maxPdu;
            _tdpuSize = tdpuSize;
            _ip = ip;
            connectTryCount = 0;
        }

        public override byte[] SendReceive(params object[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(content));
        }

        public override async Task<byte[]> SendReceiveAsync(params object[] content)
        {
            if (ProtocalLinker == null || !ProtocalLinker.IsConnected)
            {
                await ConnectAsync();
            }
            return await base.SendReceiveAsync(content);
        }

        public override OutputStruct SendReceive(ProtocalUnit unit, InputStruct content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(unit, content));
        }

        public override async Task<OutputStruct> SendReceiveAsync(ProtocalUnit unit, InputStruct content)
        {
            if (ProtocalLinker == null || !ProtocalLinker.IsConnected)
            {
                if (connectTryCount > 10) return null;
                return await await ConnectAsync().ContinueWith(answer => answer.Result ? base.SendReceiveAsync(unit, content) : null);
            }
            return await base.SendReceiveAsync(unit, content);
        }

        private async Task<OutputStruct> ForceSendReceiveAsync(ProtocalUnit unit, InputStruct content)
        {
            return await base.SendReceiveAsync(unit, content);
        }

        public override bool Connect()
        {
            return AsyncHelper.RunSync(ConnectAsync);
        }

        public override async Task<bool> ConnectAsync()
        {
            connectTryCount++;
            ProtocalLinker = new SiemensTcpProtocalLinker(_ip);
            if (await ProtocalLinker.ConnectAsync())
            {
                connectTryCount = 0;
                var inputStruct = new CreateReferenceSiemensInputStruct(_tdpuSize, _taspSrc, _tsapDst);
                return
                    await await
                        ForceSendReceiveAsync(this[typeof (CreateReferenceSiemensProtocal)], inputStruct)
                            .ContinueWith(async answer =>
                            {
                                if (!ProtocalLinker.IsConnected) return false;
                                var inputStruct2 = new EstablishAssociationSiemensInputStruct(0x0101, _maxCalling,
                                    _maxCalled,
                                    _maxPdu);
                                var outputStruct2 =
                                    (EstablishAssociationSiemensOutputStruct)
                                        await
                                            SendReceiveAsync(this[typeof (EstablishAssociationSiemensProtocal)],
                                                inputStruct2);
                                return true;
                            });
            }
            return false;
        }
    }
}
