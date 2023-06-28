namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus标准AddressFormater
    /// </summary>
    public class AddressFormaterModbus : AddressFormater<int, int>
    {
        /// <summary>
        ///     格式化地址
        /// </summary>
        /// <param name="area">地址区域</param>
        /// <param name="address">地址</param>
        /// <returns>格式化的地址字符串</returns>
        public override string FormatAddress(string area, int address)
        {
            return area + " " + address;
        }

        /// <summary>
        ///     格式化地址
        /// </summary>
        /// <param name="area">地址区域</param>
        /// <param name="address">地址</param>
        /// <param name="subAddress">比特位地址</param>
        /// <returns>格式化的地址字符串</returns>
        public override string FormatAddress(string area, int address, int subAddress)
        {
            return area + " " + address + "." + subAddress;
        }
    }
}