using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.FBox
{
    public class FBoxSignalRProtocal : FBoxProtocal
    {
        public FBoxSignalRProtocal(string machineId, string localSequence, SignalRSigninMsg msg)
        {
            ProtocalLinker = new FBoxSignalRProtocalLinker(machineId, localSequence, msg);
        }
    }
}
