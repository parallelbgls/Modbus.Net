using System;
using System.Collections.Generic;

namespace ModBus.Net
{
    /// <summary>
    /// 数据单元翻译器
    /// </summary>
    public abstract class AddressTranslator
    {
        /// <summary>
        /// 地址转换
        /// </summary>
        /// <param name="address">地址前地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <returns>Key为转换后的地址，Value为辅助码</returns>
        public abstract KeyValuePair<int,int> AddressTranslate(string address, bool isRead);
    }

    /// <summary>
    /// NA200H数据单元翻译器
    /// </summary>
    public class AddressTranslatorNA200H : AddressTranslator
    {
        protected Dictionary<string, int> TransDictionary;
        protected Dictionary<string, int> ReadFunctionCodeDictionary;
        protected Dictionary<string, int> WriteFunctionCodeDictionary;
 
        public AddressTranslatorNA200H()
        {
            TransDictionary = new Dictionary<string, int>
            {
                {"Q", 0},
                {"M", 10000},
                {"N", 20000},
                {"I", 0},
                {"S", 10000},
                {"IW", 0},
                {"SW", 5000},
                {"MW", 0},
                {"NW", 10000},
                {"QW", 20000},
            };
            ReadFunctionCodeDictionary = new Dictionary<string, int>
            {
                {"Q", (int)ModbusProtocalReadDataFunctionCode.ReadCoilStatus},
                {"M", (int)ModbusProtocalReadDataFunctionCode.ReadCoilStatus},
                {"N", (int)ModbusProtocalReadDataFunctionCode.ReadCoilStatus},
                {"I", (int)ModbusProtocalReadDataFunctionCode.ReadInputStatus},
                {"S", (int)ModbusProtocalReadDataFunctionCode.ReadInputStatus},
                {"IW", (int)ModbusProtocalReadDataFunctionCode.ReadInputRegister},
                {"SW", (int)ModbusProtocalReadDataFunctionCode.ReadInputRegister},
                {"MW", (int)ModbusProtocalReadDataFunctionCode.ReadHoldRegister},
                {"NW", (int)ModbusProtocalReadDataFunctionCode.ReadHoldRegister},
                {"QW", (int)ModbusProtocalReadDataFunctionCode.ReadHoldRegister},
            };
            WriteFunctionCodeDictionary = new Dictionary<string, int>
            {
                {"Q", (int)ModbusProtocalWriteDataFunctionCode.WriteMultiCoil},
                {"M", (int)ModbusProtocalWriteDataFunctionCode.WriteMultiCoil},
                {"N", (int)ModbusProtocalWriteDataFunctionCode.WriteMultiCoil},
                {"MW", (int)ModbusProtocalWriteDataFunctionCode.WriteMultiRegister},
                {"NW", (int)ModbusProtocalWriteDataFunctionCode.WriteMultiRegister},
                {"QW", (int)ModbusProtocalWriteDataFunctionCode.WriteMultiRegister},
            };
        }

        public override KeyValuePair<int, int> AddressTranslate(string address, bool isRead)
        {
            address = address.ToUpper();
            int i = 0;
            int t;
            while (!int.TryParse(address[i].ToString(), out t) && i < address.Length)
            {
                i++;
            }
            if (i == 0 || i >= address.Length) throw new FormatException();
            string head = address.Substring(0, i);
            string tail = address.Substring(i);
            return isRead
                ? new KeyValuePair<int, int>(TransDictionary[head] + int.Parse(tail) - 1,
                    ReadFunctionCodeDictionary[head])
                : new KeyValuePair<int, int>(TransDictionary[head] + int.Parse(tail) - 1,
                    WriteFunctionCodeDictionary[head]);
        }
    }

    /// <summary>
    /// 基本的单元转换器
    /// </summary>
    public class AddressTranslatorBase : AddressTranslator
    {
        public override KeyValuePair<int, int> AddressTranslate(string address, bool isRead)
        {
            int num1,num2;
            string[] split = address.Split(':');
            if (split.Length != 2) throw new FormatException();
            if (int.TryParse(split[0], out num1) && int.TryParse(split[1], out num2))
            {
                return new KeyValuePair<int, int>(num2, num1);
            }
            throw new FormatException();
        }
    }

    public class AddressTranslatorSimense : AddressTranslator
    {
        protected Dictionary<string, int> AreaCodeDictionary;

        public AddressTranslatorSimense()
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

        public override KeyValuePair<int, int> AddressTranslate(string address, bool isRead)
        {
            address = address.ToUpper();
            if (address.Substring(0,2) == "DB")
            {
                var addressSplit = address.Split('.');
                if (addressSplit.Length != 2) throw new FormatException();
                addressSplit[0] = addressSplit[0].Substring(2);
                if (addressSplit[1].Substring(0, 2) == "DB")
                    addressSplit[1] = addressSplit[1].Substring(2);
                return new KeyValuePair<int, int>(int.Parse(addressSplit[1]), int.Parse(addressSplit[0]) * 256 + AreaCodeDictionary["DB"]);
            }
            int i = 0;
            int t;
            while (!int.TryParse(address[i].ToString(), out t) && i < address.Length)
            {
                i++;
            }
            if (i == 0 || i >= address.Length) throw new FormatException();
            string head = address.Substring(0, i);
            string tail = address.Substring(i);
            return 
                new KeyValuePair<int, int>(int.Parse(tail),
                    AreaCodeDictionary[head]);
        }
    }
}