using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    /// <summary>
    /// NA200H数据单元翻译器
    /// </summary>
    public class AddressTranslatorNA200H : AddressTranslator
    {
        protected Dictionary<string, int> TransDictionary;
        protected Dictionary<string, AreaOutputDef> ReadFunctionCodeDictionary;
        protected Dictionary<string, AreaOutputDef> WriteFunctionCodeDictionary;

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
            ReadFunctionCodeDictionary = new Dictionary<string, AreaOutputDef>
            {
                {"Q", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadCoilStatus, AreaWidth = 0.125}},
                {"M", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadCoilStatus, AreaWidth = 0.125}},
                {"N", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadCoilStatus, AreaWidth = 0.125}},
                {"I", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadInputStatus, AreaWidth = 0.125}},
                {"S", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadInputStatus, AreaWidth = 0.125}},
                {"IW", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadInputRegister, AreaWidth = 2}},
                {"SW", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadInputRegister, AreaWidth = 2}},
                {"MW", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadHoldRegister, AreaWidth = 2}},
                {"NW", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadHoldRegister, AreaWidth = 2}},
                {"QW", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadHoldRegister, AreaWidth = 2}},
            };
            WriteFunctionCodeDictionary = new Dictionary<string, AreaOutputDef>
            {
                {"Q", new AreaOutputDef {Code = (int)ModbusProtocalWriteDataFunctionCode.WriteMultiCoil, AreaWidth = 0.125}},
                {"M", new AreaOutputDef {Code = (int)ModbusProtocalWriteDataFunctionCode.WriteMultiCoil, AreaWidth = 0.125}},
                {"N", new AreaOutputDef {Code = (int)ModbusProtocalWriteDataFunctionCode.WriteMultiCoil, AreaWidth = 0.125}},
                {"MW", new AreaOutputDef {Code = (int)ModbusProtocalWriteDataFunctionCode.WriteMultiRegister, AreaWidth = 2}},
                {"NW", new AreaOutputDef {Code = (int)ModbusProtocalWriteDataFunctionCode.WriteMultiRegister, AreaWidth = 2}},
                {"QW", new AreaOutputDef {Code = (int)ModbusProtocalWriteDataFunctionCode.WriteMultiRegister, AreaWidth = 2}},
            };
        }

        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            address = address.ToUpper();
            string[] splitString = address.Split(' ');
            string head = splitString[0];
            string tail = splitString[1];
            return isRead
                ? new AddressDef()
                {
                    Area = ReadFunctionCodeDictionary[head].Code,
                    Address = TransDictionary[head] + int.Parse(tail) - 1,
                }
                : new AddressDef()
                {
                    Area = WriteFunctionCodeDictionary[head].Code,
                    Address = TransDictionary[head] + int.Parse(tail) - 1,
                };
        }

        public override double GetAreaByteLength(string area)
        {
            return ReadFunctionCodeDictionary[area].AreaWidth;
        }
    }

    /// <summary>
    /// Modbus数据单元翻译器
    /// </summary>
    public class AddressTranslatorModbus : AddressTranslator
    {
        protected Dictionary<string, AreaOutputDef> ReadFunctionCodeDictionary;
        protected Dictionary<string, AreaOutputDef> WriteFunctionCodeDictionary;

        public AddressTranslatorModbus()
        {
            ReadFunctionCodeDictionary = new Dictionary<string, AreaOutputDef>
            {
                {"0X", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadCoilStatus, AreaWidth = 0.125}},
                {"1X", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadInputStatus, AreaWidth = 0.125}},
                {"3X", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadInputRegister, AreaWidth = 2}},
                {"4X", new AreaOutputDef {Code = (int)ModbusProtocalReadDataFunctionCode.ReadHoldRegister, AreaWidth = 2}},
            };
            WriteFunctionCodeDictionary = new Dictionary<string, AreaOutputDef>
            {
                {"0X", new AreaOutputDef {Code = (int)ModbusProtocalWriteDataFunctionCode.WriteMultiCoil, AreaWidth = 0.125}},
                {"4X", new AreaOutputDef {Code = (int)ModbusProtocalWriteDataFunctionCode.WriteMultiRegister, AreaWidth = 2}},
            };
        }

        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            address = address.ToUpper();
            string[] splitString = address.Split(' ');
            string head = splitString[0];
            string tail = splitString[1];
            return isRead
                ? new AddressDef()
                {
                    Area = ReadFunctionCodeDictionary[head].Code,
                    Address = int.Parse(tail) - 1,
                }
                : new AddressDef()
                {
                    Area = WriteFunctionCodeDictionary[head].Code,
                    Address = int.Parse(tail) - 1,
                };
        }

        public override double GetAreaByteLength(string area)
        {
            return ReadFunctionCodeDictionary[area].AreaWidth;
        }
    }
}
