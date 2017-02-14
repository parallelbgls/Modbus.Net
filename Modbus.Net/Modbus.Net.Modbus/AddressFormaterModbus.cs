namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     南大奥拓NA200H专用AddressFormater
    /// </summary>
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

    /// <summary>
    ///     Modbus标准AddressFormater
    /// </summary>
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