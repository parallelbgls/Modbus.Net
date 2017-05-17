namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     Siemens地址格式化（Modbus.Net专用格式）
    /// </summary>
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

    /// <summary>
    ///     Siemens地址格式化（Siemens格式）
    /// </summary>
    public class AddressFormaterSimenseStandard : AddressFormater
    {
        public override string FormatAddress(string area, int address)
        {
            if (area.Length > 1 &&
                area.ToUpper().Substring(0, 2) == "DB")
                return area.ToUpper() + "." + "DB" + address;
            return area.ToUpper() + address;
        }

        public override string FormatAddress(string area, int address, int subAddress)
        {
            if (area.Length > 1 &&
                area.ToUpper().Substring(0, 2) == "DB")
                return area.ToUpper() + "." + "DB" + address + "." + subAddress;
            return area.ToUpper() + address + "." + subAddress;
        }
    }
}