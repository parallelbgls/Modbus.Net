namespace Modbus.Net.Modbus
{
    public class AddressFormaterNA200H : AddressFormater
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

    public class AddressFormaterModbus : AddressFormater
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