using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.FBox
{
    public class AddressFormaterFBox : AddressFormater
    {
        public override string FormatAddress(string area, int address)
        {
            return area + " " + address;
        }
    }
}
