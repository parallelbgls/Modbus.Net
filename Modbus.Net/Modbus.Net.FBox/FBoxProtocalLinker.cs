using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.FBox
{
    public class FBoxProtocalLinker : ProtocalLinker
    {
        protected FBoxProtocalLinker(string machineId, string localSequence, SignalRSigninMsg msg)
        {
            _baseConnector = new FBoxConnector(machineId, localSequence, msg);
        }

        public override bool CheckRight(byte[] content)
        {
            if (content != null && content.Length == 6 && Encoding.ASCII.GetString(content) == "NoData")
            {
                return false;
            }
            return base.CheckRight(content);
        }
    }
}
