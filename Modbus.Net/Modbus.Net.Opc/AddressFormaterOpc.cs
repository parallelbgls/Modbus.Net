using System;
using System.Linq;

namespace Modbus.Net.Opc
{
    /// <summary>
    ///     Opc地址编码器
    /// </summary>
    public class AddressFormaterOpc<TMachineKey, TUnitKey, TAddressKey, TSubAddressKey> : AddressFormater<TAddressKey, TSubAddressKey> where TMachineKey : IEquatable<TMachineKey>
        where TUnitKey : IEquatable<TUnitKey> where TAddressKey : IEquatable<TAddressKey> where TSubAddressKey : IEquatable<TSubAddressKey>
    {
        /// <summary>
        ///     协议构造器
        /// </summary>
        /// <param name="tagGeter">如何通过BaseMachine和AddressUnit构造Opc的标签</param>
        /// <param name="machine">调用这个编码器的设备</param>
        public AddressFormaterOpc(Func<BaseMachine<TMachineKey, TUnitKey, TAddressKey, TSubAddressKey>, AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>, string> tagGeter,
            BaseMachine<TMachineKey, TUnitKey, TAddressKey, TSubAddressKey> machine)
        {
            Machine = machine;
            TagGeter = tagGeter;
        }

        /// <summary>
        ///     设备
        /// </summary>
        public BaseMachine<TMachineKey, TUnitKey, TAddressKey, TSubAddressKey> Machine { get; set; }

        /// <summary>
        ///     标签构造器
        ///     (设备,地址)->不具备分隔符的标签数组
        /// </summary>
        protected Func<BaseMachine<TMachineKey, TUnitKey, TAddressKey, TSubAddressKey>, AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>, string> TagGeter { get; set; }

        /// <summary>
        ///     编码地址
        /// </summary>
        /// <param name="area">地址所在的数据区域</param>
        /// <param name="address">地址</param>
        /// <returns>编码后的地址</returns>
        public override string FormatAddress(string area, TAddressKey address)
        {
            var findAddress = Machine?.GetAddresses.FirstOrDefault(p => p.Area == area && p.Address.Equals(address));
            if (findAddress == null) return null;
            var ans = TagGeter(Machine, findAddress);
            ans = ans.Trim().Replace(" ", "");
            return ans;
        }

        /// <summary>
        ///     编码地址
        /// </summary>
        /// <param name="area">地址所在的数据区域</param>
        /// <param name="address">地址</param>
        /// <param name="subAddress">子地址（忽略）</param>
        /// <returns>编码后的地址</returns>
        public override string FormatAddress(string area, TAddressKey address, TSubAddressKey subAddress)
        {
            return FormatAddress(area, address);
        }
    }
}