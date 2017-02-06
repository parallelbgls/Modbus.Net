using System;
using System.Linq;

namespace Modbus.Net.OPC
{
    public class AddressFormaterOpc : AddressFormater
    {
        public AddressFormaterOpc(Func<BaseMachine, AddressUnit, string[]> tagGeter, BaseMachine machine,
            char seperator = '/')
        {
            Machine = machine;
            TagGeter = tagGeter;
            Seperator = seperator;
        }

        public BaseMachine Machine { get; set; }

        protected Func<BaseMachine, AddressUnit, string[]> TagGeter { get; set; }

        protected char Seperator { get; set; }

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