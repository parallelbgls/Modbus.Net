using System.Configuration;
using System.Threading.Tasks;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子Ppi协议
    /// </summary>
    public class SiemensPpiProtocal : SiemensProtocal
    {
        private readonly string _com;

        public SiemensPpiProtocal(byte slaveAddress, byte masterAddress)
            : this(ConfigurationManager.AppSettings["COM"], slaveAddress, masterAddress)
        {
        }

        public SiemensPpiProtocal(string com, byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress)
        {
            _com = com;
        }

        public override byte[] SendReceive(params object[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(Endian, content));
        }

        public override async Task<byte[]> SendReceiveAsync(params object[] content)
        {
            if (ProtocalLinker == null || !ProtocalLinker.IsConnected)
            {
                await ConnectAsync();
            }
            return await base.SendReceiveAsync(Endian, content);
        }

        private async Task<IOutputStruct> ForceSendReceiveAsync(ProtocalUnit unit, IInputStruct content)
        {
            return await base.SendReceiveAsync(unit, content);
        }

        public override bool Connect()
        {
            return AsyncHelper.RunSync(() => ConnectAsync());
        }

        public override async Task<bool> ConnectAsync()
        {
            ProtocalLinker = new SiemensPpiProtocalLinker(_com);
            var inputStruct = new ComCreateReferenceSiemensInputStruct(SlaveAddress, MasterAddress);
            var outputStruct =
                await await
                    ForceSendReceiveAsync(this[typeof (ComCreateReferenceSiemensProtocal)],
                        inputStruct).
                        ContinueWith(async answer =>
                        {
                            if (!ProtocalLinker.IsConnected) return false;
                            var inputStruct2 = new ComConfirmMessageSiemensInputStruct(SlaveAddress, MasterAddress);
                            var outputStruct2 =
                                (ComConfirmMessageSiemensOutputStruct)
                                    await
                                        ForceSendReceiveAsync(this[typeof (ComConfirmMessageSiemensProtocal)],
                                            inputStruct2);
                            return outputStruct2 != null;
                        });
            return outputStruct;
        }
    }
}