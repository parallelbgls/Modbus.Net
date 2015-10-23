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
                {"DB", 10000},
            };
        }

        public override KeyValuePair<int, int> AddressTranslate(string address, bool isRead)
        {
            var tmpAddress = address.Replace(" ", "");
            tmpAddress = tmpAddress.ToUpper();
            if (tmpAddress.Substring(0, 2) == "DB")
            {
                var addressSplit = tmpAddress.Split('.');
                if (addressSplit.Length != 2) throw new FormatException();
                addressSplit[0] = addressSplit[0].Substring(2);
                if (addressSplit[1].Substring(0, 2) == "DB")
                    addressSplit[1] = addressSplit[1].Substring(2);
                return new KeyValuePair<int, int>(int.Parse(addressSplit[1]),
                    int.Parse(addressSplit[0]) + AreaCodeDictionary["DB"]);
            }
            int i = 0;
            int t;
            while (!int.TryParse(tmpAddress[i].ToString(), out t) && i < tmpAddress.Length)
            {
                i++;
            }
            if (i == 0 || i >= tmpAddress.Length) throw new FormatException();
            string head = tmpAddress.Substring(0, i);
            string tail = tmpAddress.Substring(i);
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
