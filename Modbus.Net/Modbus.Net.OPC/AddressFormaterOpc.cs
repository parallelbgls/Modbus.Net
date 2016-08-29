using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    public class AddressFormaterOpc : AddressFormater
    {
        public BaseMachine Machine { get; set; }

        protected Func<BaseMachine, AddressUnit, string[]> TagGeter { get; set; }

        public AddressFormaterOpc(Func<BaseMachine, AddressUnit, string[]> tagGeter)
        {
            TagGeter = tagGeter;
        }

        public override string FormatAddress(string area, int address)
        {
            var findAddress = Machine?.GetAddresses.FirstOrDefault(p => p.Area == area && p.Address == address);
            if (findAddress == null) return null;
            var strings = TagGeter(Machine, findAddress);
            var ans = "";
            for (int i = 0; i < strings.Length; i++)
            {
                ans += strings[i].Trim().Replace(" ", "") + ".";
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
