using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Siemens
{
    public class SiemensPpiProtocal : SiemensProtocal
    {
        private readonly string _com;
        private int _connectTryCount;

        public SiemensPpiProtocal() : this( ConfigurationManager.COM)
        {
        }

        public SiemensPpiProtocal(string com)
        {
            _com = com;
            _connectTryCount = 0;
        }

        public override byte[] SendReceive(bool isLittleEndian, params object[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(isLittleEndian, content));
        }

        public override async Task<byte[]> SendReceiveAsync(bool isLittleEndian, params object[] content)
        {
            if (ProtocalLinker == null || !ProtocalLinker.IsConnected)
            {
                await ConnectAsync();
            }
            return await base.SendReceiveAsync(isLittleEndian, content);
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
            ProtocalLinker = new SiemensPpiProtocalLinker(_com);
            var inputStruct = new ComCreateReferenceSiemensInputStruct();
            var outputStruct =
                await await
                        ForceSendReceiveAsync(this[typeof (ComCreateReferenceSiemensProtocal)],
                            inputStruct).
                            ContinueWith(async answer =>
                            {
                                if (!ProtocalLinker.IsConnected) return false;
                                var inputStruct2 = new ComEstablishAssociationSiemensInputStruct();
                                var outputStruct2 =
                                    (ComConfirmSiemensOutputStruct)
                                        await
                                            ForceSendReceiveAsync(this[typeof(ComEstablishAssociationSiemensProtocal)],
                                                inputStruct2);
                                return outputStruct2 != null;
                            });
            return outputStruct != null;
        }
    }
}
