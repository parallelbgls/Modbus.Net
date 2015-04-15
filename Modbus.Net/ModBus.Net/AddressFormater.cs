using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public abstract class AddressFormater
    {
        public abstract string FormatAddress(string area, int address);
    }

    public class AddressFormaterSimense : AddressFormater
    {
        public override string FormatAddress(string area, int address)
        {
            if (area.ToUpper() == "DB")
            {
                return area.ToUpper() + "." + "DB" + address;
            }
            else
            {
                return area.ToUpper() + address;
            }
        }
    }

    public class AddressFormaterBase : AddressFormater
    {
        public override string FormatAddress(string area, int address)
        {
            return area + "." + address;
        }
    }

    public class AddressFormaterNA200H : AddressFormater
    {
        public override string FormatAddress(string area, int address)
        {
            return area + address;
        }
    }
}
