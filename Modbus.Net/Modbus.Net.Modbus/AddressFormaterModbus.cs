using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    public class AddressFormaterNA200H : AddressFormater
    {
        public override string FormatAddress(string area, int address)
        {
            return area + " " + address;
        }
    }

    public class AddressFormaterModbus : AddressFormater
    {
        public override string FormatAddress(string area, int address)
        {
            return area + " " + address;
        }
    }
}
