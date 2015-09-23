using System;

namespace ModBus.Net
{
    public abstract class AddressFormater
    {
        public abstract string FormatAddress(string area, int address);
    }

    public class AddressFormaterBase : AddressFormater
    {
        public override string FormatAddress(string area, int address)
        {
            return area + "." + address;
        }
    }
}
