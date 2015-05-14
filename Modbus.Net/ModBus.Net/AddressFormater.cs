using System;

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
            if (area.Length > 1 && 
                area.ToUpper().Substring(0,2) == "DB")
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
