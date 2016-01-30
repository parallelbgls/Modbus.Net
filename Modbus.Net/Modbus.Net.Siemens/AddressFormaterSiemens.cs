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
            /*
            if (area.Length > 1 &&
                area.ToUpper().Substring(0, 2) == "DB")
            {
                return area.ToUpper() + "." + "DB" + address;
            }
            else
            {
                return area.ToUpper() + address;
            }
            */
            return area + " " + address;
        }
    }
}
