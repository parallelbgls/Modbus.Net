using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net.Modbus
{
    public abstract class ModbusTranslatorBase : AddressTranslator
    {
        /// <summary>
        ///     地址转换
        /// </summary>
        /// <param name="address">格式化的地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <param name="isSingle">是否只写入一个数据</param>
        /// <returns>翻译后的地址</returns>
        public abstract AddressDef AddressTranslate(string address, bool isRead, bool isSingle);

        /// <summary>
        ///     地址转换
        /// </summary>
        /// <param name="address">格式化的地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <returns>翻译后的地址</returns>
        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            return AddressTranslate(address, isRead, false);
        }
    }

    /// <summary>
    ///     南大奥拓NA200H数据单元翻译器
    /// </summary>
    public class AddressTranslatorNA200H : ModbusTranslatorBase
    {
        /// <summary>
        ///     读功能码
        /// </summary>
        protected Dictionary<string, AreaOutputDef> ReadFunctionCodeDictionary;

        /// <summary>
        ///     功能码翻译至标准Modbus地址位置
        /// </summary>
        protected Dictionary<string, int> TransDictionary;

        /// <summary>
        ///     写功能码
        /// </summary>
        protected Dictionary<(string, bool), AreaOutputDef> WriteFunctionCodeDictionary;

        /// <summary>
        ///     构造器
        /// </summary>
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
                {"NW", 21000}
            };
            ReadFunctionCodeDictionary = new Dictionary<string, AreaOutputDef>
            {
                {
                    "Q",
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalReadDataFunctionCode.ReadCoilStatus,
                        AreaWidth = 0.125
                    }
                },
                {
                    "M",
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalReadDataFunctionCode.ReadCoilStatus,
                        AreaWidth = 0.125
                    }
                },
                {
                    "N",
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalReadDataFunctionCode.ReadCoilStatus,
                        AreaWidth = 0.125
                    }
                },
                {
                    "I",
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalReadDataFunctionCode.ReadInputStatus,
                        AreaWidth = 0.125
                    }
                },
                {
                    "S",
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalReadDataFunctionCode.ReadInputStatus,
                        AreaWidth = 0.125
                    }
                },
                {
                    "IW",
                    new AreaOutputDef {Code = (int) ModbusProtocalReadDataFunctionCode.ReadInputRegister, AreaWidth = 2}
                },
                {
                    "SW",
                    new AreaOutputDef {Code = (int) ModbusProtocalReadDataFunctionCode.ReadInputRegister, AreaWidth = 2}
                },
                {
                    "MW",
                    new AreaOutputDef {Code = (int) ModbusProtocalReadDataFunctionCode.ReadHoldRegister, AreaWidth = 2}
                },
                {
                    "NW",
                    new AreaOutputDef {Code = (int) ModbusProtocalReadDataFunctionCode.ReadHoldRegister, AreaWidth = 2}
                },
                {
                    "QW",
                    new AreaOutputDef {Code = (int) ModbusProtocalReadDataFunctionCode.ReadHoldRegister, AreaWidth = 2}
                }
            };
            WriteFunctionCodeDictionary = new Dictionary<(string, bool), AreaOutputDef>
            {
                {
                    ("Q", false),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteMultiCoil,
                        AreaWidth = 0.125
                    }
                },
                {
                    ("M", false),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteMultiCoil,
                        AreaWidth = 0.125
                    }
                },
                {
                    ("N", false),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteMultiCoil,
                        AreaWidth = 0.125
                    }
                },
                {
                    ("MW", false),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteMultiRegister,
                        AreaWidth = 2
                    }
                },
                {
                    ("NW", false),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteMultiRegister,
                        AreaWidth = 2
                    }
                },
                {
                    ("QW", false),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteMultiRegister,
                        AreaWidth = 2
                    }
                },
                {
                    ("Q", true),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteSingleCoil,
                        AreaWidth = 0.125
                    }
                },
                {
                    ("M", true),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteSingleCoil,
                        AreaWidth = 0.125
                    }
                },
                {
                    ("N", true),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteSingleCoil,
                        AreaWidth = 0.125
                    }
                },
                {
                    ("MW", true),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteSingleRegister,
                        AreaWidth = 2
                    }
                },
                {
                    ("NW", true),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteSingleRegister,
                        AreaWidth = 2
                    }
                },
                {
                    ("QW", true),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteSingleRegister,
                        AreaWidth = 2
                    }
                }
            };
        }

        /// <summary>
        ///     地址转换
        /// </summary>
        /// <param name="address">格式化的地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <param name="isSingle">是否只写入一个数据</param>
        /// <returns>翻译后的地址</returns>
        public override AddressDef AddressTranslate(string address, bool isRead, bool isSingle)
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
            return isRead
                ? new AddressDef
                {
                    AreaString = head,
                    Area = ReadFunctionCodeDictionary[head].Code,
                    Address = TransDictionary[head] + int.Parse(tail) - 1,
                    SubAddress = int.Parse(sub)
                }
                : new AddressDef
                {
                    AreaString = head,
                    Area = WriteFunctionCodeDictionary[(head, isSingle)].Code,
                    Address = TransDictionary[head] + int.Parse(tail) - 1,
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
            return ReadFunctionCodeDictionary[area].AreaWidth;
        }
    }

    /// <summary>
    ///     Modbus数据单元翻译器
    /// </summary>
    public class AddressTranslatorModbus : ModbusTranslatorBase
    {
        /// <summary>
        ///     读功能码
        /// </summary>
        protected Dictionary<string, AreaOutputDef> ReadFunctionCodeDictionary;

        /// <summary>
        ///     写功能码
        /// </summary>
        protected Dictionary<(string, bool), AreaOutputDef> WriteFunctionCodeDictionary;

        /// <summary>
        ///     构造器
        /// </summary>
        public AddressTranslatorModbus()
        {
            ReadFunctionCodeDictionary = new Dictionary<string, AreaOutputDef>
            {
                {
                    "0X",
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalReadDataFunctionCode.ReadCoilStatus,
                        AreaWidth = 0.125
                    }
                },
                {
                    "1X",
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalReadDataFunctionCode.ReadInputStatus,
                        AreaWidth = 0.125
                    }
                },
                {
                    "3X",
                    new AreaOutputDef {Code = (int) ModbusProtocalReadDataFunctionCode.ReadInputRegister, AreaWidth = 2}
                },
                {
                    "4X",
                    new AreaOutputDef {Code = (int) ModbusProtocalReadDataFunctionCode.ReadHoldRegister, AreaWidth = 2}
                }
            };
            WriteFunctionCodeDictionary = new Dictionary<(string, bool), AreaOutputDef>
            {
                {
                    ("0X", false),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteMultiCoil,
                        AreaWidth = 0.125
                    }
                },
                {
                    ("4X", false),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteMultiRegister,
                        AreaWidth = 2
                    }
                },
                {
                    ("0X", true),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteSingleCoil,
                        AreaWidth = 0.125
                    }
                },
                {
                    ("4X", true),
                    new AreaOutputDef
                    {
                        Code = (int) ModbusProtocalWriteDataFunctionCode.WriteSingleRegister,
                        AreaWidth = 2
                    }
                }
            };
        }

        /// <summary>
        ///     地址转换
        /// </summary>
        /// <param name="address">格式化的地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <param name="isSingle">是否只写入一个数据</param>
        /// <returns>翻译后的地址</returns>
        public override AddressDef AddressTranslate(string address, bool isRead, bool isSingle)
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
            return isRead
                ? new AddressDef
                {
                    AreaString = head,
                    Area = ReadFunctionCodeDictionary[head].Code,
                    Address = int.Parse(tail) - 1,
                    SubAddress = int.Parse(sub)
                }
                : new AddressDef
                {
                    AreaString = head,
                    Area = WriteFunctionCodeDictionary[(head, isSingle)].Code,
                    Address = int.Parse(tail) - 1,
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
            return ReadFunctionCodeDictionary[area].AreaWidth;
        }
    }
}