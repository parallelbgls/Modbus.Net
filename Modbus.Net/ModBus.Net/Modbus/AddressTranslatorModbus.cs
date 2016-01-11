using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.Modbus
{
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
                {"N", 30000},
                {"I", 0},
                {"S", 10000},
                {"IW", 0},
                {"SW", 5000},
                {"MW", 0},                
                {"QW", 20000},
                {"NW", 21000},
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
            string[] splitString = address.Split(' ');
            string head = splitString[0];
            string tail = splitString[1];
            return isRead
                ? new KeyValuePair<int, int>(TransDictionary[head] + int.Parse(tail) - 1,
                    ReadFunctionCodeDictionary[head])
                : new KeyValuePair<int, int>(TransDictionary[head] + int.Parse(tail) - 1,
                    WriteFunctionCodeDictionary[head]);
        }
    }

    /// <summary>
    /// Modbus数据单元翻译器
    /// </summary>
    public class AddressTranslatorModbus : AddressTranslator
    {
        protected Dictionary<string, int> ReadFunctionCodeDictionary;
        protected Dictionary<string, int> WriteFunctionCodeDictionary;

        public AddressTranslatorModbus()
        {
            ReadFunctionCodeDictionary = new Dictionary<string, int>
            {
                {"0X", (int)ModbusProtocalReadDataFunctionCode.ReadCoilStatus},
                {"1X", (int)ModbusProtocalReadDataFunctionCode.ReadInputStatus},
                {"3X", (int)ModbusProtocalReadDataFunctionCode.ReadInputRegister},
                {"4X", (int)ModbusProtocalReadDataFunctionCode.ReadHoldRegister},
            };
            WriteFunctionCodeDictionary = new Dictionary<string, int>
            {
                {"0X", (int)ModbusProtocalWriteDataFunctionCode.WriteMultiCoil},
                {"4X", (int)ModbusProtocalWriteDataFunctionCode.WriteMultiRegister},
            };
        }

        public override KeyValuePair<int, int> AddressTranslate(string address, bool isRead)
        {
            address = address.ToUpper();
            string[] splitString = address.Split(' ');
            string head = splitString[0];
            string tail = splitString[1];
            return isRead
                ? new KeyValuePair<int, int>(int.Parse(tail) - 1,
                    ReadFunctionCodeDictionary[head])
                : new KeyValuePair<int, int>(int.Parse(tail) - 1,
                    WriteFunctionCodeDictionary[head]);
        }
    }
}
