using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    public class AddressTranslatorOpc : AddressTranslator
    {
        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            throw new NotImplementedException();
        }

        public override double GetAreaByteLength(string area)
        {
            return 1;
        }
    }
}
