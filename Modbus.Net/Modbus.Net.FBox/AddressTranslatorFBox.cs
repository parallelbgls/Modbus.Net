using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.FBox
{
    public class AddressTranslatorFBox : AddressTranslator
    {
        protected Dictionary<string, int> AreaCodeDictionary;

        public AddressTranslatorFBox()
        {
            AreaCodeDictionary = new Dictionary<string, int>
            {
                {"V", 1},
                {"VW", 2},
                {"VD", 3},
                {"V.B", 4},
                {"I", 5},
                {"IW", 6},
                {"ID", 7},
                {"I.B", 8},
                {"Q", 9},
                {"QW", 10},
                {"QD", 11},
                {"Q.B", 12},
                {"M", 13},
                {"MW", 14},
                {"MD", 15},
                {"M.B", 16},
                {"0X", 20},
                {"1X", 21},
                {"2X", 22},
                {"3X", 23},
                {"4X", 24},
                {"DB", 10000},
            };
        }

        public override KeyValuePair<int, int> AddressTranslate(string address, bool isRead)
        {
            var tmpAddress = address.Trim().ToUpper();
            if (tmpAddress.Substring(0, 2) == "DB")
            {
                var addressSplit = tmpAddress.Split(' ');
                if (addressSplit.Length != 2) throw new FormatException();
                addressSplit[0] = addressSplit[0].Substring(2);
                return new KeyValuePair<int, int>(int.Parse(addressSplit[1]),
                    int.Parse(addressSplit[0]) + AreaCodeDictionary["DB"]);
            }
            var tmpAddressArray = (tmpAddress.Split(' '));
            string head = tmpAddressArray[0];
            string tail = tmpAddressArray[1];
            return
                new KeyValuePair<int, int>(int.Parse(tail),
                    AreaCodeDictionary[head]);
        }

        public string GetAreaName(int code)
        {
            if (code < 10000)
                return AreaCodeDictionary.FirstOrDefault(p => p.Value == code).Key;
            else
                return AreaCodeDictionary.FirstOrDefault(p => p.Value == code - code%10000).Key + code%10000;
        }
    }
}
