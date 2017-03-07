using System;
using System.Linq;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc地址编码器
    /// </summary>
    public class AddressFormaterOpc<TMachineKey,TUnitKey> : AddressFormater where TMachineKey : IEquatable<TMachineKey> where TUnitKey : IEquatable<TUnitKey>
    {
        /// <summary>
        ///     协议构造器
        /// </summary>
        /// <param name="tagGeter">如何通过BaseMachine和AddressUnit构造Opc的标签</param>
        /// <param name="machine">调用这个编码器的设备</param>
        /// <param name="seperator">每两个标签之间用什么符号隔开，默认为/</param>
        public AddressFormaterOpc(Func<BaseMachine<TMachineKey, TUnitKey>, AddressUnit<TUnitKey>, string[]> tagGeter, BaseMachine<TMachineKey, TUnitKey> machine,
            char seperator = '/')
        {
            Machine = machine;
            TagGeter = tagGeter;
            Seperator = seperator;
        }

        public BaseMachine<TMachineKey, TUnitKey> Machine { get; set; }

        protected Func<BaseMachine<TMachineKey, TUnitKey>, AddressUnit<TUnitKey>, string[]> TagGeter { get; set; }

        public char Seperator { get; protected set; }

        public override string FormatAddress(string area, int address)
        {
            var findAddress = Machine?.GetAddresses.FirstOrDefault(p => p.Area == area && p.Address == address);
            if (findAddress == null) return null;
            var strings = TagGeter(Machine, findAddress);
            var ans = "";
            for (var i = 0; i < strings.Length; i++)
            {
                ans += strings[i].Trim().Replace(" ", "") + Seperator;
            }
            ans = ans.Substring(0, ans.Length - 1);
            return ans;
        }

        public override string FormatAddress(string area, int address, int subAddress)
        {
            return FormatAddress(area, address);
        }
    }
}