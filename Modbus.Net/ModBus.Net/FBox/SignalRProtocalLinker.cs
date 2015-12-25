using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.FBox
{
    public class SignalRProtocalLinker : ProtocalLinker
    {
        protected SignalRProtocalLinker(string machineId, string localSequence, SignalRSigninMsg msg)
        {
            _baseConnector = new SignalRConnector(machineId, localSequence, msg);
        }
    }
}
