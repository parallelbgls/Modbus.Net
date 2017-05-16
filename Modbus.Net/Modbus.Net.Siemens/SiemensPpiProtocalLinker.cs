using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子Ppi协议连接器
    /// </summary>
    public class SiemensPpiProtocalLinker : ComProtocalLinker
    {
        public SiemensPpiProtocalLinker(string com, int slaveAddress)
            : base(com, 9600, Parity.Even, StopBits.One, 8, slaveAddress)
        {
        }

        public override async Task<byte[]> SendReceiveAsync(byte[] content)
        {
            var extBytes = BytesExtend(content);
            if (extBytes[6] == 0x7c)
            {
                var inputStruct2 = new ComConfirmMessageSiemensInputStruct(content[4], content[5]);
                var receiveBytes2 =
                    await SendReceiveWithoutExtAndDecAsync(
                        new ComConfirmMessageSiemensProtocal().Format(inputStruct2));
            }
            var receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
            if (content.Length > 6 && receiveBytes.Length == 1 && receiveBytes[0] == 0xe5)
            {
                var inputStruct2 = new ComConfirmMessageSiemensInputStruct(content[4], content[5]);
                var receiveBytes2 =
                    await SendReceiveWithoutExtAndDecAsync(
                        new ComConfirmMessageSiemensProtocal().Format(inputStruct2));
                return BytesDecact(receiveBytes2);
            }
            return BytesDecact(receiveBytes);
        }

        public override async Task<byte[]> SendReceiveWithoutExtAndDecAsync(byte[] content)
        {
            var ans = await base.SendReceiveWithoutExtAndDecAsync(content);
            while (ans.Length == 1 && ans[0] == 0xf9)
            {
                Thread.Sleep(500);
                if (content.Length <= 6)
                {
                    var inputStruct2 = new ComConfirmMessageSiemensInputStruct(content[1], content[2]);
                    ans =
                        await SendReceiveWithoutExtAndDecAsync(
                            new ComConfirmMessageSiemensProtocal().Format(inputStruct2));
                }
                else
                {
                    var inputStruct2 = new ComConfirmMessageSiemensInputStruct(content[4], content[5]);
                    ans =
                        await SendReceiveWithoutExtAndDecAsync(
                            new ComConfirmMessageSiemensProtocal().Format(inputStruct2));
                }
            }
            return ans;
        }

        public override bool? CheckRight(byte[] content)
        {
            if (!base.CheckRight(content).Value) return false;
            var fcsCheck = 0;
            if (content.Length == 1 && content[0] == 0xe5)
            {
                return true;
            }
            if (content.Length == 6 && content[3] == 0) return true;
            for (var i = 4; i < content.Length - 2; i++)
            {
                fcsCheck += content[i];
            }
            fcsCheck = fcsCheck%256;
            if (fcsCheck != content[content.Length - 2]) return false;
            if (content[content.Length - 1] != 0x16) return false;
            if (content[1] != content.Length - 6) return false;
            return true;
        }
    }
}