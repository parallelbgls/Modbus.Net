using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.FBox
{
    public class SignalRProtocalLinker : ProtocalLinker
    {
        protected SignalRProtocalLinker(string machineId, SignalRSigninMsg msg)
        {
            _baseConnector = new SignalRConnector(machineId, msg);
        }
    }
}
