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

        public SiemensPpiProtocal(byte belongAddress, byte masterAddress) : this( ConfigurationManager.COM, belongAddress, masterAddress)
        {
        }

        public SiemensPpiProtocal(string com, byte belongAddress, byte masterAddress) : base(belongAddress, masterAddress)
        {
            _com = com;
        }

        public override byte[] SendReceive(bool isLittleEndian, params object[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(isLittleEndian, content));
        }

        private async Task<OutputStruct> ForceSendReceiveAsync(ProtocalUnit unit, InputStruct content)
        {
            return await base.SendReceiveAsync(unit, content);
        }

        public override bool Connect()
        {
            return AsyncHelper.RunSync(()=>ConnectAsync());
        }

        public override async Task<bool> ConnectAsync()
        {
            ProtocalLinker = new SiemensPpiProtocalLinker(_com);
            var inputStruct = new ComCreateReferenceSiemensInputStruct(BelongAddress, MasterAddress);
            var outputStruct =
                await await
                        ForceSendReceiveAsync(this[typeof (ComCreateReferenceSiemensProtocal)],
                            inputStruct).
                            ContinueWith(async answer =>
                            {
                                if (!ProtocalLinker.IsConnected) return false;
                                var inputStruct2 = new ComConfirmMessageSiemensInputStruct(BelongAddress, MasterAddress);
                                var outputStruct2 =
                                    (ComConfirmMessageSiemensOutputStruct)
                                        await
                                            ForceSendReceiveAsync(this[typeof(ComConfirmMessageSiemensProtocal)],
                                                inputStruct2);
                                return outputStruct2 != null;
                            });
            return outputStruct != null;
        }
    }
}
