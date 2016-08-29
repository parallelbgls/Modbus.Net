using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Siemens
{
    public class AddressFormaterSiemens : AddressFormater
    {
        public override string FormatAddress(string area, int address)
        {
            return area + " " + address;
        }

        public override string FormatAddress(string area, int address, int subAddress)
        {
            return area + " " + address + "." + subAddress;
        }
    }
}
