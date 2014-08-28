using System.Collections.Generic;

namespace ModBus.Net
{
    /// <summary>
    /// 数据单元翻译器
    /// </summary>
    public abstract class AddressTranslator
    {
        protected static AddressTranslator _instance;
        public Dictionary<string, short> TransDictionary;

        public abstract ushort AddressTranslate(string address);
    }

    /// <summary>
    /// NA200H数据单元翻译器
    /// </summary>
    public class AddressTranslatorNA200H : AddressTranslator
    {
        private AddressTranslatorNA200H()
        {
            TransDictionary = new Dictionary<string, short>();
            TransDictionary.Add("Q", 0);
            TransDictionary.Add("M", 10000);
            TransDictionary.Add("N", 20000);
            TransDictionary.Add("I", 0);
            TransDictionary.Add("S", 10000);
            TransDictionary.Add("IW", 0);
            TransDictionary.Add("SW", 5000);
            TransDictionary.Add("E", 10000);
            TransDictionary.Add("MW", 0);
            TransDictionary.Add("NW", 10000);
            TransDictionary.Add("QW", 20000);
            TransDictionary.Add("CLOCK", 30000);
            TransDictionary.Add("V", 0);
        }

        public static AddressTranslator GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AddressTranslatorNA200H();
            }
            return _instance;
        }

        public override ushort AddressTranslate(string address)
        {
            address = address.ToUpper();
            int i = 0;
            int t;
            while (!int.TryParse(address[i].ToString(), out t) && i < address.Length)
            {
                i++;
            }
            if (i == 0) return ushort.Parse(address);
            string head = address.Substring(0, i);
            string tail = address.Substring(i);
            return (ushort) (TransDictionary[head] + ushort.Parse(tail) - 1);
        }
    }
}