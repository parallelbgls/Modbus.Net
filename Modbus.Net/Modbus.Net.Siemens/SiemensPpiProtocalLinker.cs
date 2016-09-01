using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net.Siemens
{
    public class SiemensPpiProtocalLinker : ComProtocalLinker
    {
        public override async Task<byte[]> SendReceiveAsync(byte[] content)
        {
            byte[] extBytes = BytesExtend(content);
            if (extBytes[6] == 0x7c)
            {
                var inputStruct2 = new ComEstablishAssociationSiemensInputStruct();
                var receiveBytes2 =
                    await SendReceiveWithoutExtAndDecAsync(
                        new ComEstablishAssociationSiemensProtocal().Format(inputStruct2));
            }
            var receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
            while (receiveBytes.Length == 1 && receiveBytes[0] == 0xf9)
            {
                Thread.Sleep(500);
                var inputStruct2 = new ComEstablishAssociationSiemensInputStruct();
                receiveBytes =
                    await SendReceiveWithoutExtAndDecAsync(
                        new ComEstablishAssociationSiemensProtocal().Format(inputStruct2));
            }
            if (content.Length > 6 && receiveBytes.Length == 1 && receiveBytes[0] == 0xe5)
            {
                var inputStruct2 = new ComEstablishAssociationSiemensInputStruct();
                var receiveBytes2 =
                    await SendReceiveWithoutExtAndDecAsync(
                        new ComEstablishAssociationSiemensProtocal().Format(inputStruct2));
                return BytesDecact(receiveBytes2);
            }
            return BytesDecact(receiveBytes);
        }

        public override bool? CheckRight(byte[] content)
        {
            if (!base.CheckRight(content).Value) return false;
            int fcsCheck = 0;
            if (content.Length == 1 && content[0] == 0xe5)
            {
                return true;
            }
            if (content.Length == 6 && content[3] == 0) return true;
            for (int i = 4; i < content.Length - 2; i++)
            {
                fcsCheck += content[i];
            }
            fcsCheck = fcsCheck%256;
            if (fcsCheck != content[content.Length - 2]) return false;
            if (content[content.Length - 1] != 0x16) return false;
            if (content[1] != content.Length - 6) return false;
            return true;
        }

        public SiemensPpiProtocalLinker(string com)
            : base(com, 9600, Parity.Even, StopBits.One, 8)
        {
        }
    }
}
