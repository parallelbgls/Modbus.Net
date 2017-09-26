namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     Siemens地址格式化（Modbus.Net专用格式）
    /// </summary>
    public class AddressFormaterSiemens : AddressFormater
    {
        /// <summary>
        ///     编码地址
        /// </summary>
        /// <param name="area">地址所在的数据区域</param>
        /// <param name="address">地址</param>
        /// <returns>编码后的地址</returns>
        public override string FormatAddress(string area, int address)
        {
            return area + " " + address;
        }

        /// <summary>
        ///     编码地址
        /// </summary>
        /// <param name="area">地址所在的数据区域</param>
        /// <param name="address">地址</param>
        /// <param name="subAddress">子地址</param>
        /// <returns>编码后的地址</returns>
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
        /// <summary>
        ///     编码地址
        /// </summary>
        /// <param name="area">地址所在的数据区域</param>
        /// <param name="address">地址</param>
        /// <returns>编码后的地址</returns>
        public override string FormatAddress(string area, int address)
        {
            if (area.Length > 1 &&
                area.ToUpper().Substring(0, 2) == "DB")
                return area.ToUpper() + "." + "DB" + address;
            return area.ToUpper() + address;
        }

        /// <summary>
        ///     编码地址
        /// </summary>
        /// <param name="area">地址所在的数据区域</param>
        /// <param name="address">地址</param>
        /// <param name="subAddress">子地址</param>
        /// <returns>编码后的地址</returns>
        public override string FormatAddress(string area, int address, int subAddress)
        {
            if (area.Length > 1 &&
                area.ToUpper().Substring(0, 2) == "DB")
                return area.ToUpper() + "." + "DB" + address + "." + subAddress;
            return area.ToUpper() + address + "." + subAddress;
        }
    }
}