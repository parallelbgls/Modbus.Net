using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     地址翻译器（Modbus.Net格式）
    /// </summary>
    public class AddressTranslatorSiemens : AddressTranslator
    {
        /// <summary>
        ///     区域的翻译字典
        /// </summary>
        protected Dictionary<string, int> AreaCodeDictionary;

        /// <summary>
        ///     构造函数
        /// </summary>
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
                {"V", 0x184}
            };
        }

        /// <summary>
        ///     地址转换
        /// </summary>
        /// <param name="address">格式化的地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <returns>翻译后的地址</returns>
        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            address = address.ToUpper();
            var splitString = address.Split(' ');
            var head = splitString[0];
            var tail = splitString[1];
            string sub;
            if (tail.Contains('.'))
            {
                var splitString2 = tail.Split('.');
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
                return new AddressDef
                {
                    AreaString = "DB" + head,
                    Area = int.Parse(head) * 256 + AreaCodeDictionary["DB"],
                    Address = int.Parse(tail),
                    SubAddress = int.Parse(sub)
                };
            }
            return
                new AddressDef
                {
                    AreaString = head,
                    Area = AreaCodeDictionary[head],
                    Address = int.Parse(tail),
                    SubAddress = int.Parse(sub)
                };
        }

        /// <summary>
        ///     获取区域中的单个地址占用的字节长度
        /// </summary>
        /// <param name="area">区域名称</param>
        /// <returns>字节长度</returns>
        public override double GetAreaByteLength(string area)
        {
            return 1;
        }
    }

    /// <summary>
    ///     地址翻译器（Siemens格式）
    /// </summary>
    public class AddressTranslatorSimenseStandard : AddressTranslator
    {
        /// <summary>
        ///     区域的翻译字典
        /// </summary>
        protected Dictionary<string, int> AreaCodeDictionary;

        /// <summary>
        ///     构造函数
        /// </summary>
        public AddressTranslatorSimenseStandard()
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
                {"V", 0x184}
            };
        }

        /// <summary>
        ///     地址转换
        /// </summary>
        /// <param name="address">格式化的地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <returns>翻译后的地址</returns>
        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            address = address.ToUpper();
            if (address.Substring(0, 2) == "DB")
            {
                var addressSplit = address.Split('.');
                if (addressSplit.Length != 2 && addressSplit.Length != 3) throw new FormatException();
                addressSplit[0] = addressSplit[0].Substring(2);
                if (addressSplit[1].Substring(0, 2) == "DB")
                    addressSplit[1] = addressSplit[1].Substring(2);
                return new AddressDef
                {
                    AreaString = "DB" + addressSplit[0],
                    Area = int.Parse(addressSplit[0]) * 256 + AreaCodeDictionary["DB"],
                    Address = int.Parse(addressSplit[1]),
                    SubAddress = addressSplit.Length == 2 ? 0 : int.Parse(addressSplit[2])
                };
            }
            var i = 0;
            int t;
            while (!int.TryParse(address[i].ToString(), out t) && i < address.Length)
                i++;
            if (i == 0 || i >= address.Length) throw new FormatException();
            var head = address.Substring(0, i);
            var tail = address.Substring(i).Split('.');
            return new AddressDef
            {
                AreaString = head,
                Area = AreaCodeDictionary[head],
                Address = int.Parse(tail[0]),
                SubAddress = tail.Length == 1 ? 0 : int.Parse(tail[1])
            };
        }

        /// <summary>
        ///     获取区域中的单个地址占用的字节长度
        /// </summary>
        /// <param name="area">区域名称</param>
        /// <returns>字节长度</returns>
        public override double GetAreaByteLength(string area)
        {
            return 1;
        }
    }
}