using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.FBox
{
    public class FBoxSignalRProtocalLinker : SignalRProtocalLinker
    {
        public FBoxSignalRProtocalLinker(string machineId, SignalRSigninMsg msg) : base(machineId, msg)
        {
        }

        public override bool CheckRight(byte[] content)
        {
            return true;
        }
    }
}
