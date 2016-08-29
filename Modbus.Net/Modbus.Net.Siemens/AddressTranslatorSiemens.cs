using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net.Siemens
{
    public class AddressTranslatorSiemens : AddressTranslator
    {
        protected Dictionary<string, int> AreaCodeDictionary;

        public AddressTranslatorSiemens()
        {
            AreaCodeDictionary = new Dictionary<string, int>
            {
                {"S", 0x04},
                {"SM", 0x05},
                {"AI", 0x06},
                {"AQ", 0x07},
                {"C", 0x1E},
                {"T", 0x1F},
                {"HC", 0x20},
                {"I", 0x81},
                {"Q", 0x82},
                {"M", 0x83},
                {"DB", 0x84},
                {"V", 0x184},
            };
        }

        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            address = address.ToUpper();
            string[] splitString = address.Split(' ');
            string head = splitString[0];
            string tail = splitString[1];
            string sub;
            if (tail.Contains('.'))
            {
                string[] splitString2 = tail.Split('.');
                sub = splitString2[1];
                tail = splitString2[0];
            }
            else
            {
                sub = "0";
            }
            if (head.Length > 1 && head.Substring(0, 2) == "DB")
            {
                head = head.Substring(2);
                return new AddressDef()
                {
                    Area = int.Parse(head)*256 + AreaCodeDictionary["DB"],
                    Address = int.Parse(tail),
                    SubAddress = int.Parse(sub),
                };
            }
            return 
                new AddressDef()
                {
                    Area = AreaCodeDictionary[head],
                    Address = int.Parse(tail),
                    SubAddress = int.Parse(sub),
                };
        }

        public override double GetAreaByteLength(string area)
        {
            return 1;
        }
    }
}