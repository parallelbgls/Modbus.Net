using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.Net
{
    /// <summary>
    ///     值与字节数组之间转换的辅助类
    /// </summary>
    public abstract class ValueHelper
    {
        /// <summary>
        ///     兼容数据类型对应的字节长度
        /// </summary>
        /// <summary>
        ///     兼容数据类型对应的字节长度
        /// </summary>
        public static Dictionary<string, double> ByteLength => new Dictionary<string, double>
        {
            {"System.Boolean", 0.125},
            {"System.Byte", 1},
            {"System.Int16", 2},
            {"System.Int32", 4},
            {"System.Int64", 8},
            {"System.UInt16", 2},
            {"System.UInt32", 4},
            {"System.UInt64", 8},
            {"System.Single", 4},
            {"System.Double", 8}
        };

        /// <summary>
        ///     将一个byte数字转换为一个byte元素的数组。
        /// </summary>
        /// <param name="value">byte数字</param>
        /// <returns>byte数组</returns>
        public abstract byte[] GetBytes(byte value);

        /// <summary>
        ///     将short数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public abstract byte[] GetBytes(short value);

        /// <summary>
        ///     将int数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public abstract byte[] GetBytes(int value);

        /// <summary>
        ///     将long数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public abstract byte[] GetBytes(long value);

        /// <summary>
        ///     将ushort数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public abstract byte[] GetBytes(ushort value);

        /// <summary>
        ///     将uint数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public abstract byte[] GetBytes(uint value);

        /// <summary>
        ///     将ulong数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public abstract byte[] GetBytes(ulong value);

        /// <summary>
        ///     将float数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public abstract byte[] GetBytes(float value);

        /// <summary>
        ///     将double数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public abstract byte[] GetBytes(double value);

        /// <summary>
        ///     将object数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <param name="type">待转换的值的类型</param>
        /// <returns>转换后的byte数组</returns>
        public abstract byte[] GetBytes(object value, Type type);

        /// <summary>
        ///     将byte数组中相应的位置转换为对应类型的数字
        /// </summary>
        /// <param name="data">待转换的字节数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <param name="subPos">转换数字的比特位置（仅Type为bool的时候有效）</param>
        /// <param name="t">转换的类型</param>
        /// <returns>转换出的数字</returns>
        public abstract object GetValue(byte[] data, ref int pos, ref int subPos, Type t);

        /// <summary>
        ///     将byte数组中相应的位置转换为byte数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public abstract byte GetByte(byte[] data, ref int pos);

        /// <summary>
        ///     将byte数组中相应的位置转换为short数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public abstract short GetShort(byte[] data, ref int pos);

        /// <summary>
        ///     将byte数组中相应的位置转换为int数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public abstract int GetInt(byte[] data, ref int pos);

        /// <summary>
        ///     将byte数组中相应的位置转换为long数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public abstract long GetLong(byte[] data, ref int pos);

        /// <summary>
        ///     将byte数组中相应的位置转换为ushort数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public abstract ushort GetUShort(byte[] data, ref int pos);

        /// <summary>
        ///     将byte数组中相应的位置转换为uint数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public abstract uint GetUInt(byte[] data, ref int pos);

        /// <summary>
        ///     将byte数组中相应的位置转换为ulong数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public abstract ulong GetULong(byte[] data, ref int pos);

        /// <summary>
        ///     将byte数组中相应的位置转换为float数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public abstract float GetFloat(byte[] data, ref int pos);

        /// <summary>
        ///     将byte数组中相应的位置转换为double数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public abstract double GetDouble(byte[] data, ref int pos);

        /// <summary>
        ///     将byte数组中相应的位置转换为字符串
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="count">转换的个数</param>
        /// <param name="pos">转换数字的位置</param>
        /// <param name="encoding">编码方法</param>
        /// <returns>转换出的字符串</returns>
        public abstract string GetString(byte[] data, int count, ref int pos, Encoding encoding);

        /// <summary>
        ///     将byte数组中相应的位置转换为8个bit数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的位数组</returns>
        public abstract bool[] GetBits(byte[] data, ref int pos);

        /// <summary>
        ///     获取一个byte中相对应的bit数组展开中第n个位置中的bit元素。
        /// </summary>
        /// <param name="number">byte数字</param>
        /// <param name="pos">bit数组中的对应位置</param>
        /// <param name="subPos">小数位</param>
        /// <returns>对应位置的bit元素</returns>
        public abstract bool GetBit(byte number, ref int pos, ref int subPos);

        /// <summary>
        ///     获取一个字节数组中某个Bit位的数据
        /// </summary>
        /// <param name="number">byte数字</param>
        /// <param name="pos">bit数组中的对应位置</param>
        /// <param name="subPos">小数位</param>
        /// <returns>对应位置的bit元素</returns>
        public abstract bool GetBit(byte[] number, ref int pos, ref int subPos);

        /// <summary>
        ///     反转一个字节的8个Bit位
        /// </summary>
        /// <param name="originalByte">原始bit数组</param>
        /// <returns>反转的bit数组</returns>
        public abstract byte ReverseByte(byte originalByte);

        /// <summary>
        ///     将待转换的对象数组转换为需要发送的byte数组
        /// </summary>
        /// <param name="contents">object数组</param>
        /// <returns>byte数组</returns>
        public abstract byte[] ObjectArrayToByteArray(object[] contents);

        /// <summary>
        ///     将byte数组转换为用户指定类型的数组，通过object数组的方式返回，用户需要再把object转换为自己需要的类型，或调用ObjectArrayToDestinationArray返回单一类型的目标数组。
        /// </summary>
        /// <param name="contents">byte数组</param>
        /// <param name="translateTypeAndCount">单一的类型和需要转换的个数的键值对</param>
        /// <returns>object数组</returns>
        public abstract object[] ByteArrayToObjectArray(byte[] contents, KeyValuePair<Type, int> translateTypeAndCount);

        /// <summary>
        ///     将一个byte数组转换成用户指定类型的数组，使用模板参数确定需要转换的类型
        /// </summary>
        /// <typeparam name="T">目标数组类型</typeparam>
        /// <param name="contents">待转换的数组</param>
        /// <param name="getCount">转换的个数</param>
        /// <returns>以T为类型的数组</returns>
        public abstract T[] ByteArrayToDestinationArray<T>(byte[] contents, int getCount);

        /// <summary>
        ///     将byte数组转换为用户指定类型的数组，通过object数组的方式返回，用户需要再把object转换为自己需要的类型，或调用ObjectArrayToDestinationArray返回单一类型的目标数组。
        /// </summary>
        /// <param name="contents">byte数组</param>
        /// <param name="translateTypeAndCount">
        ///     一连串类型和需要转换的个数的键值对，该方法会依次转换每一个需要转的目标数据类型。比如：typeof(int),5; typeof(short),3
        ///     会转换出8个元素（当然前提是byte数组足够长的时候），5个int和3个short，然后全部变为object类型返回。
        /// </param>
        /// <returns>object数组</returns>
        public abstract object[] ByteArrayToObjectArray(byte[] contents, IEnumerable<KeyValuePair<Type, int>> translateTypeAndCount);

        /// <summary>
        ///     将object数组转换为目标类型的单一数组
        /// </summary>
        /// <typeparam name="T">需要转换的目标类型</typeparam>
        /// <param name="contents">object数组</param>
        /// <returns>目标数组</returns>
        public abstract T[] ObjectArrayToDestinationArray<T>(object[] contents);

        /// <summary>
        ///     在一个数组中写一个值
        /// </summary>
        /// <param name="contents">待写入的字节数组</param>
        /// <param name="setPos">设置的位置</param>
        /// <param name="subPos">设置的比特位位置（仅setValue为bit的时候有效）</param>
        /// <param name="setValue">写入的值</param>
        /// <returns>写入是否成功</returns>
        public abstract bool SetValue(byte[] contents, int setPos, int subPos, object setValue);

        /// <summary>
        ///     将short值设置到byte数组中
        /// </summary>
        /// <param name="contents">待设置的byte数组</param>
        /// <param name="pos">设置的位置</param>
        /// <param name="setValue">要设置的值</param>
        /// <returns>设置是否成功</returns>
        public abstract bool SetValue(byte[] contents, int pos, object setValue);

        /// <summary>
        ///     设置一组数据中的一个bit
        /// </summary>
        /// <param name="contents">待设置的字节数组</param>
        /// <param name="pos">位置</param>
        /// <param name="subPos">bit位置</param>
        /// <param name="setValue">bit数</param>
        /// <returns>设置是否成功</returns>
        public abstract bool SetBit(byte[] contents, int pos, int subPos, bool setValue);

        /// <summary>
        ///     根据端格式获取ValueHelper实例
        /// </summary>
        /// <param name="endian">端格式</param>
        /// <returns>对应的ValueHelper实例</returns>
        public static ValueHelper GetInstance(Endian endian)
        {
            if (EndianInstanceCache.ContainsKey(endian.ToString()))
            {
                return EndianInstanceCache[endian.ToString()];
            }
            foreach (var assembly in AssemblyHelper.GetAllLibraryAssemblies())
            {
                var valueHelper = assembly.GetType("Modbus.Net." + endian.ToString() + "ValueHelper")?.GetProperty("Instance")?.GetValue(null, null) as ValueHelper;
                if (valueHelper != null)
                {
                    EndianInstanceCache[endian.ToString()] = valueHelper;
                    return valueHelper;
                }

            }
            throw new NotImplementedException("ValueHelper " + endian.ToString() + " doesn't exist.");
        }

        /// <summary>
        ///     ValueHelper实例的缓存
        /// </summary>
        protected static Dictionary<string, ValueHelper> EndianInstanceCache = new Dictionary<string, ValueHelper>();
    }

    /// <summary>
    ///     值与字节数组之间转换的辅助类（小端格式），这是一个Singleton类
    /// </summary>
    public class LittleEndianLsbValueHelper : ValueHelper
    {
        private static readonly ILogger<LittleEndianLsbValueHelper> logger = LogProvider.CreateLogger<LittleEndianLsbValueHelper>();

        /// <summary>
        ///     构造器
        /// </summary>
        protected LittleEndianLsbValueHelper()
        {
        }

        /// <summary>
        ///     协议中的内容构造是否小端的，默认是小端构造协议。
        /// </summary>
        public static bool LittleEndian => true;

        /// <summary>
        ///     协议中的比特位内容构造是否小端的，默认是小端构造协议。
        /// </summary>
        public static bool LittleEndianBit => true;

        /// <summary>
        ///     将一个byte数字转换为一个byte元素的数组。
        /// </summary>
        /// <param name="value">byte数字</param>
        /// <returns>byte数组</returns>
        public override byte[] GetBytes(byte value)
        {
            return new[] { value };
        }

        /// <summary>
        ///     将short数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(short value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        ///     将int数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        ///     将long数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        ///     将ushort数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        ///     将uint数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        ///     将ulong数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(ulong value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        ///     将float数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        ///     将double数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        ///     将object数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <param name="type">待转换的值的类型</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(object value, Type type)
        {
            switch (type.FullName)
            {
                case "System.Int16":
                    {
                        var bytes = _Instance.GetBytes((short)value);
                        return bytes;
                    }
                case "System.Int32":
                    {
                        var bytes = _Instance.GetBytes((int)value);
                        return bytes;
                    }
                case "System.Int64":
                    {
                        var bytes = _Instance.GetBytes((long)value);
                        return bytes;
                    }
                case "System.UInt16":
                    {
                        var bytes = _Instance.GetBytes((ushort)value);
                        return bytes;
                    }
                case "System.UInt32":
                    {
                        var bytes = _Instance.GetBytes((uint)value);
                        return bytes;
                    }
                case "System.UInt64":
                    {
                        var bytes = _Instance.GetBytes((ulong)value);
                        return bytes;
                    }
                case "System.Single":
                    {
                        var bytes = _Instance.GetBytes((float)value);
                        return bytes;
                    }
                case "System.Double":
                    {
                        var bytes = _Instance.GetBytes((double)value);
                        return bytes;
                    }
                case "System.Byte":
                    {
                        var bytes = _Instance.GetBytes((byte)value);
                        return bytes;
                    }
                case "System.Boolean":
                    {
                        var bytes = _Instance.GetBytes((bool)value ? (byte)1 : (byte)0);
                        return bytes;
                    }
                default:
                    {
                        throw new NotImplementedException("没有实现除整数以外的其它转换方式");
                    }
            }
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为对应类型的数字
        /// </summary>
        /// <param name="data">待转换的字节数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <param name="subPos">转换数字的比特位置（仅Type为bool的时候有效）</param>
        /// <param name="t">转换的类型</param>
        /// <returns>转换出的数字</returns>
        public override object GetValue(byte[] data, ref int pos, ref int subPos, Type t)
        {
            switch (t.FullName)
            {
                case "System.Int16":
                    {
                        var value = _Instance.GetShort(data, ref pos);
                        return value;
                    }
                case "System.Int32":
                    {
                        var value = _Instance.GetInt(data, ref pos);
                        return value;
                    }
                case "System.Int64":
                    {
                        var value = _Instance.GetLong(data, ref pos);
                        return value;
                    }
                case "System.UInt16":
                    {
                        var value = _Instance.GetUShort(data, ref pos);
                        return value;
                    }
                case "System.UInt32":
                    {
                        var value = _Instance.GetUInt(data, ref pos);
                        return value;
                    }
                case "System.UInt64":
                    {
                        var value = _Instance.GetULong(data, ref pos);
                        return value;
                    }
                case "System.Single":
                    {
                        var value = _Instance.GetFloat(data, ref pos);
                        return value;
                    }
                case "System.Double":
                    {
                        var value = _Instance.GetDouble(data, ref pos);
                        return value;
                    }
                case "System.Byte":
                    {
                        var value = _Instance.GetByte(data, ref pos);
                        return value;
                    }
                case "System.Boolean":
                    {
                        var value = _Instance.GetBit(data, ref pos, ref subPos);
                        return value;
                    }
                default:
                    {
                        throw new NotImplementedException("没有实现除整数以外的其它转换方式");
                    }
            }
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为byte数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override byte GetByte(byte[] data, ref int pos)
        {
            var t = data[pos];
            pos += 1;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为short数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override short GetShort(byte[] data, ref int pos)
        {
            var t = BitConverter.ToInt16(data, pos);
            pos += 2;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为int数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override int GetInt(byte[] data, ref int pos)
        {
            var t = BitConverter.ToInt32(data, pos);
            pos += 4;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为long数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override long GetLong(byte[] data, ref int pos)
        {
            var t = BitConverter.ToInt64(data, pos);
            pos += 8;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为ushort数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override ushort GetUShort(byte[] data, ref int pos)
        {
            var t = BitConverter.ToUInt16(data, pos);
            pos += 2;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为uint数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override uint GetUInt(byte[] data, ref int pos)
        {
            var t = BitConverter.ToUInt32(data, pos);
            pos += 4;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为ulong数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override ulong GetULong(byte[] data, ref int pos)
        {
            var t = BitConverter.ToUInt64(data, pos);
            pos += 8;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为float数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override float GetFloat(byte[] data, ref int pos)
        {
            var t = BitConverter.ToSingle(data, pos);
            pos += 4;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为double数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override double GetDouble(byte[] data, ref int pos)
        {
            var t = BitConverter.ToDouble(data, pos);
            pos += 8;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为字符串
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="count">转换的个数</param>
        /// <param name="pos">转换数字的位置</param>
        /// <param name="encoding">编码方法</param>
        /// <returns>转换出的字符串</returns>
        public override string GetString(byte[] data, int count, ref int pos, Encoding encoding)
        {
            var t = encoding.GetString(data, pos, count);
            pos += count;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为8个bit数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的位数组</returns>
        public override bool[] GetBits(byte[] data, ref int pos)
        {
            var t = new bool[8];
            var temp = data[pos];
            for (var i = 0; i < 8; i++)
            {
                t[i] = temp % 2 > 0;
                temp /= 2;
            }
            pos += 1;
            return t;
        }

        /// <summary>
        ///     获取一个byte中相对应的bit数组展开中第n个位置中的bit元素。
        /// </summary>
        /// <param name="number">byte数字</param>
        /// <param name="pos">bit数组中的对应位置</param>
        /// <param name="subPos">小数位</param>
        /// <returns>对应位置的bit元素</returns>
        public override bool GetBit(byte number, ref int pos, ref int subPos)
        {
            if (subPos < 0 && subPos >= 8) throw new IndexOutOfRangeException();
            var ans = number % 2;
            var i = 0;
            while (i <= subPos)
            {
                ans = number % 2;
                number /= 2;
                i++;
            }
            subPos += 1;
            if (subPos > 7)
            {
                pos++;
                subPos = 0;
            }
            return ans > 0;
        }

        /// <summary>
        ///     获取一个字节数组中某个Bit位的数据
        /// </summary>
        /// <param name="number">byte数字</param>
        /// <param name="pos">bit数组中的对应位置</param>
        /// <param name="subPos">小数位</param>
        /// <returns>对应位置的bit元素</returns>
        public override bool GetBit(byte[] number, ref int pos, ref int subPos)
        {
            return GetBit(number[pos], ref pos, ref subPos);
        }

        /// <summary>
        ///     反转一个字节的8个Bit位
        /// </summary>
        /// <param name="originalByte">原始bit数组</param>
        /// <returns>反转的bit数组</returns>
        public override byte ReverseByte(byte originalByte)
        {
            byte result = 0;
            for (var i = 0; i < 8; i++)
            {
                result <<= 1;
                result |= (byte)(originalByte & 1);
                originalByte >>= 1;
            }
            return result;
        }

        /// <summary>
        ///     将待转换的对象数组转换为需要发送的byte数组
        /// </summary>
        /// <param name="contents">object数组</param>
        /// <returns>byte数组</returns>
        public override byte[] ObjectArrayToByteArray(object[] contents)
        {
            var b = false;
            //先查找传入的结构中有没有数组，有的话将其打开
            var newContentsList = new List<object>();
            foreach (var content in contents)
            {
                var t = content.GetType().ToString();
                if (t.Substring(t.Length - 2, 2) == "[]")
                {
                    b = true;
                    //自动将目标数组中内含的子数组展开，是所有包含在子数组拼接为一个数组
                    var contentArray =
                        ArrayList.Adapter((Array)content).ToArray(typeof(object)).OfType<object>();
                    newContentsList.AddRange(contentArray);
                }
                else
                {
                    newContentsList.Add(content);
                }
            }
            //重新调用一边这个函数，这个传入的参数中一定没有数组。
            if (b) return ObjectArrayToByteArray(newContentsList.ToArray());
            //把参数一个一个翻译为相对应的字节，然后拼成一个队列
            var translateTarget = new List<byte>();
            //将bool类型拼装为byte类型时，按照8个一组，不满8个时补false为原则进行
            var lastIsBool = false;
            byte boolToByteTemp = 0;
            var boolToByteCount = 0;
            foreach (var content in contents)
            {
                var t = content.GetType().ToString();
                if (t == "System.Boolean")
                {
                    if (boolToByteCount >= 8)
                    {
                        translateTarget.Add(boolToByteTemp);
                        boolToByteCount = 0;
                        boolToByteTemp = 0;
                    }
                    lastIsBool = true;
                    if (!LittleEndianBit)
                        boolToByteTemp = (byte)(boolToByteTemp * 2 + ((bool)content ? 1 : 0));
                    else
                        boolToByteTemp += (byte)((bool)content ? Math.Pow(2, boolToByteCount) : 0);
                    boolToByteCount++;
                }
                else
                {
                    if (lastIsBool)
                    {
                        translateTarget.Add(boolToByteTemp);
                        boolToByteCount = 0;
                        boolToByteTemp = 0;
                        lastIsBool = false;
                    }
                    switch (t)
                    {
                        case "System.Int16":
                            {
                                translateTarget.AddRange(_Instance.GetBytes((short)content));
                                break;
                            }
                        case "System.Int32":
                            {
                                translateTarget.AddRange(_Instance.GetBytes((int)content));
                                break;
                            }
                        case "System.Int64":
                            {
                                translateTarget.AddRange(_Instance.GetBytes((long)content));
                                break;
                            }
                        case "System.UInt16":
                            {
                                translateTarget.AddRange(_Instance.GetBytes((ushort)content));
                                break;
                            }
                        case "System.UInt32":
                            {
                                translateTarget.AddRange(_Instance.GetBytes((uint)content));
                                break;
                            }
                        case "System.UInt64":
                            {
                                translateTarget.AddRange(_Instance.GetBytes((ulong)content));
                                break;
                            }
                        case "System.Single":
                            {
                                translateTarget.AddRange(_Instance.GetBytes((float)content));
                                break;
                            }
                        case "System.Double":
                            {
                                translateTarget.AddRange(_Instance.GetBytes((double)content));
                                break;
                            }
                        case "System.Byte":
                            {
                                translateTarget.AddRange(_Instance.GetBytes((byte)content));
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("没有实现除整数以外的其它转换方式");
                            }
                    }
                }
            }
            //最后是bool拼装时，表示数字还没有添加，把数字添加进返回数组中。
            if (lastIsBool)
                translateTarget.Add(boolToByteTemp);
            //最后把队列转换为数组
            return translateTarget.ToArray();
        }

        /// <summary>
        ///     将byte数组转换为用户指定类型的数组，通过object数组的方式返回，用户需要再把object转换为自己需要的类型，或调用ObjectArrayToDestinationArray返回单一类型的目标数组。
        /// </summary>
        /// <param name="contents">byte数组</param>
        /// <param name="translateTypeAndCount">单一的类型和需要转换的个数的键值对</param>
        /// <returns>object数组</returns>
        public override object[] ByteArrayToObjectArray(byte[] contents, KeyValuePair<Type, int> translateTypeAndCount)
        {
            return ByteArrayToObjectArray(contents, new List<KeyValuePair<Type, int>> { translateTypeAndCount });
        }

        /// <summary>
        ///     将一个byte数组转换成用户指定类型的数组，使用模板参数确定需要转换的类型
        /// </summary>
        /// <typeparam name="T">目标数组类型</typeparam>
        /// <param name="contents">待转换的数组</param>
        /// <param name="getCount">转换的个数</param>
        /// <returns>以T为类型的数组</returns>
        public override T[] ByteArrayToDestinationArray<T>(byte[] contents, int getCount)
        {
            var objectArray = _Instance.ByteArrayToObjectArray(contents,
                new KeyValuePair<Type, int>(typeof(T), getCount));
            return _Instance.ObjectArrayToDestinationArray<T>(objectArray);
        }

        /// <summary>
        ///     将byte数组转换为用户指定类型的数组，通过object数组的方式返回，用户需要再把object转换为自己需要的类型，或调用ObjectArrayToDestinationArray返回单一类型的目标数组。
        /// </summary>
        /// <param name="contents">byte数组</param>
        /// <param name="translateTypeAndCount">
        ///     一连串类型和需要转换的个数的键值对，该方法会依次转换每一个需要转的目标数据类型。比如：typeof(int),5; typeof(short),3
        ///     会转换出8个元素（当然前提是byte数组足够长的时候），5个int和3个short，然后全部变为object类型返回。
        /// </param>
        /// <returns>object数组</returns>
        public override object[] ByteArrayToObjectArray(byte[] contents,
            IEnumerable<KeyValuePair<Type, int>> translateTypeAndCount)
        {
            var translation = new List<object>();
            var count = 0;
            foreach (var translateUnit in translateTypeAndCount)
                for (var i = 0; i < translateUnit.Value; i++)
                {
                    if (count >= contents.Length) break;
                    try
                    {
                        switch (translateUnit.Key.ToString())
                        {
                            case "System.Int16":
                                {
                                    var value = _Instance.GetShort(contents, ref count);
                                    translation.Add(value);
                                    break;
                                }
                            case "System.Int32":
                                {
                                    var value = _Instance.GetInt(contents, ref count);
                                    translation.Add(value);
                                    break;
                                }
                            case "System.Int64":
                                {
                                    var value = _Instance.GetLong(contents, ref count);
                                    translation.Add(value);
                                    break;
                                }
                            case "System.UInt16":
                                {
                                    var value = _Instance.GetUShort(contents, ref count);
                                    translation.Add(value);
                                    break;
                                }
                            case "System.UInt32":
                                {
                                    var value = _Instance.GetUInt(contents, ref count);
                                    translation.Add(value);
                                    break;
                                }
                            case "System.UInt64":
                                {
                                    var value = _Instance.GetULong(contents, ref count);
                                    translation.Add(value);
                                    break;
                                }
                            case "System.Single":
                                {
                                    var value = _Instance.GetFloat(contents, ref count);
                                    translation.Add(value);
                                    break;
                                }
                            case "System.Double":
                                {
                                    var value = _Instance.GetDouble(contents, ref count);
                                    translation.Add(value);
                                    break;
                                }
                            case "System.Byte":
                                {
                                    var value = _Instance.GetByte(contents, ref count);
                                    translation.Add(value);
                                    break;
                                }
                            case "System.Boolean":
                                {
                                    var value = _Instance.GetBits(contents, ref count);
                                    var k = translateUnit.Value - i < 8 ? translateUnit.Value - i : 8;
                                    for (var j = 0; j < k; j++)
                                        translation.Add(value[j]);
                                    i += 7;
                                    break;
                                }
                            default:
                                {
                                    throw new NotImplementedException("Number casting not implemented");
                                }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "ValueHelper -> ByteArrayToObjectArray error");
                        count = contents.Length;
                    }
                }
            return translation.ToArray();
        }

        /// <summary>
        ///     将object数组转换为目标类型的单一数组
        /// </summary>
        /// <typeparam name="T">需要转换的目标类型</typeparam>
        /// <param name="contents">object数组</param>
        /// <returns>目标数组</returns>
        public override T[] ObjectArrayToDestinationArray<T>(object[] contents)
        {
            var array = new T[contents.Length];
            Array.Copy(contents, array, contents.Length);
            return array;
        }

        /// <summary>
        ///     在一个数组中写一个值
        /// </summary>
        /// <param name="contents">待写入的字节数组</param>
        /// <param name="setPos">设置的位置</param>
        /// <param name="subPos">设置的比特位位置（仅setValue为bit的时候有效）</param>
        /// <param name="setValue">写入的值</param>
        /// <returns>写入是否成功</returns>
        public override bool SetValue(byte[] contents, int setPos, int subPos, object setValue)
        {
            var type = setValue.GetType();

            switch (type.FullName)
            {
                case "System.Int16":
                    {
                        var success = _Instance.SetValue(contents, setPos, (short)setValue);
                        return success;
                    }
                case "System.Int32":
                    {
                        var success = _Instance.SetValue(contents, setPos, (int)setValue);
                        return success;
                    }
                case "System.Int64":
                    {
                        var success = _Instance.SetValue(contents, setPos, (long)setValue);
                        return success;
                    }
                case "System.UInt16":
                    {
                        var success = _Instance.SetValue(contents, setPos, (ushort)setValue);
                        return success;
                    }
                case "System.UInt32":
                    {
                        var success = _Instance.SetValue(contents, setPos, (uint)setValue);
                        return success;
                    }
                case "System.UInt64":
                    {
                        var success = _Instance.SetValue(contents, setPos, (ulong)setValue);
                        return success;
                    }
                case "System.Single":
                    {
                        var success = _Instance.SetValue(contents, setPos, (float)setValue);
                        return success;
                    }
                case "System.Double":
                    {
                        var success = _Instance.SetValue(contents, setPos, (double)setValue);
                        return success;
                    }
                case "System.Byte":
                    {
                        var success = _Instance.SetValue(contents, setPos, (byte)setValue);
                        return success;
                    }
                case "System.Boolean":
                    {
                        var success = _Instance.SetBit(contents, setPos, subPos, (bool)setValue);
                        return success;
                    }
                default:
                    {
                        throw new NotImplementedException("Number casting not implemented");
                    }
            }
        }

        /// <summary>
        ///     将short值设置到byte数组中
        /// </summary>
        /// <param name="contents">待设置的byte数组</param>
        /// <param name="pos">设置的位置</param>
        /// <param name="setValue">要设置的值</param>
        /// <returns>设置是否成功</returns>
        public override bool SetValue(byte[] contents, int pos, object setValue)
        {
            try
            {
                var datas = _Instance.GetBytes(setValue, setValue.GetType());
                Array.Copy(datas, 0, contents, pos, datas.Length);
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "ValueHelper -> SetValue set value failed");
                return false;
            }
        }

        /// <summary>
        ///     设置对应数字中相应位置的bit的值
        /// </summary>
        /// <param name="number">byte数子</param>
        /// <param name="subPos">设置位置</param>
        /// <param name="setBit">设置bit大小，true为1，false为0</param>
        /// <returns>设置是否成功</returns>
        protected byte SetBit(byte number, int subPos, bool setBit)
        {
            var creation = 0;
            if (setBit)
            {
                for (var i = 7; i >= 0; i--)
                {
                    creation *= 2;
                    if (i == subPos) creation++;
                }
                return (byte)(number | creation);
            }
            for (var i = 7; i >= 0; i--)
            {
                creation *= 2;
                if (i != subPos) creation++;
            }
            return (byte)(number & creation);
        }

        /// <summary>
        ///     设置一组数据中的一个bit
        /// </summary>
        /// <param name="contents">待设置的字节数组</param>
        /// <param name="pos">位置</param>
        /// <param name="subPos">bit位置</param>
        /// <param name="setValue">bit数</param>
        /// <returns>设置是否成功</returns>
        public override bool SetBit(byte[] contents, int pos, int subPos, bool setValue)
        {
            try
            {
                contents[pos] = SetBit(contents[pos], subPos, setValue);
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "ValueHelper -> SetBit set bit failed");
                return false;
            }
        }

        #region Factory

        private static ValueHelper _instance;

        /// <summary>
        ///     实例，继承时请把它覆写掉
        /// </summary>
        protected virtual ValueHelper _Instance => _instance;

        /// <summary>
        ///     ValueHelper单例的实例
        /// </summary>
        public static ValueHelper Instance => _instance ?? (_instance = new LittleEndianLsbValueHelper());

        #endregion
    }

    /// <summary>
    ///     值与字节数组之间转换的辅助类（大端格式），这是一个Singleton类
    /// </summary>
    public class BigEndianLsbValueHelper : LittleEndianLsbValueHelper
    {
        private static BigEndianLsbValueHelper _bigEndianInstance;

        /// <summary>
        ///     构造器
        /// </summary>
        protected BigEndianLsbValueHelper()
        {
        }

        /// <summary>
        ///     覆写的实例获取
        /// </summary>
        protected override ValueHelper _Instance => _bigEndianInstance;

        /// <summary>
        ///     是否为大端
        /// </summary>
        protected new bool LittleEndian => false;

        /// <summary>
        ///     覆盖的获取实例的方法
        /// </summary>
        public new static BigEndianLsbValueHelper Instance
            => _bigEndianInstance ?? (_bigEndianInstance = new BigEndianLsbValueHelper());

        /// <summary>
        ///     将short数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(short value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        /// <summary>
        ///     将int数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(int value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        /// <summary>
        ///     将long数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        /// <returns>转换出的数字</returns>
        public override byte[] GetBytes(long value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        /// <summary>
        ///     将ushort数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(ushort value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        /// <summary>
        ///     将uint数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(uint value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        /// <summary>
        ///     将ulong数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(ulong value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        /// <summary>
        ///     将float数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(float value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        /// <summary>
        ///     将double数字转换为byte数组
        /// </summary>
        /// <param name="value">待转换的值</param>
        /// <returns>转换后的byte数组</returns>
        public override byte[] GetBytes(double value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为short数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override short GetShort(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 2);
            var t = BitConverter.ToInt16(data, pos);
            Array.Reverse(data, pos, 2);
            pos += 2;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为int数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override int GetInt(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            var t = BitConverter.ToInt32(data, pos);
            Array.Reverse(data, pos, 4);
            pos += 4;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为long数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override long GetLong(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            var t = BitConverter.ToInt64(data, pos);
            Array.Reverse(data, pos, 8);
            pos += 8;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为ushort数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override ushort GetUShort(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 2);
            var t = BitConverter.ToUInt16(data, pos);
            Array.Reverse(data, pos, 2);
            pos += 2;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为uint数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override uint GetUInt(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            var t = BitConverter.ToUInt32(data, pos);
            Array.Reverse(data, pos, 4);
            pos += 4;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为ulong数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override ulong GetULong(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            var t = BitConverter.ToUInt64(data, pos);
            Array.Reverse(data, pos, 8);
            pos += 8;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为float数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override float GetFloat(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            var t = BitConverter.ToSingle(data, pos);
            Array.Reverse(data, pos, 4);
            pos += 4;
            return t;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为double数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的数字</returns>
        public override double GetDouble(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            var t = BitConverter.ToDouble(data, pos);
            Array.Reverse(data, pos, 8);
            pos += 8;
            return t;
        }

        /// <summary>
        ///     反转一个byte数组
        /// </summary>
        /// <param name="data">待反转的数组</param>
        /// <returns>反转后的数组</returns>
        private byte[] Reverse(byte[] data)
        {
            Array.Reverse(data);
            return data;
        }
    }

    /// <summary>
    ///     值与字节数组之间转换的辅助类（大端-大端位格式），这是一个Singleton类
    /// </summary>
    public class BigEndianMsbValueHelper : BigEndianLsbValueHelper
    {
        private static BigEndianMsbValueHelper _bigEndianInstance;

        /// <summary>
        ///     构造函数
        /// </summary>
        protected BigEndianMsbValueHelper()
        {
        }

        /// <summary>
        ///     覆写的实例获取方法
        /// </summary>
        protected override ValueHelper _Instance => _bigEndianInstance;

        /// <summary>
        ///     是否为小端
        /// </summary>
        protected new bool LittleEndian => false;

        /// <summary>
        ///     是否为小端位
        /// </summary>
        protected new bool LittleEndianBit => false;

        /// <summary>
        ///     覆盖的实例获取方法
        /// </summary>
        public new static BigEndianMsbValueHelper Instance
            => _bigEndianInstance ?? (_bigEndianInstance = new BigEndianMsbValueHelper());

        /// <summary>
        ///     获取一个byte中相对应的bit数组展开中第n个位置中的bit元素。
        /// </summary>
        /// <param name="number">byte数字</param>
        /// <param name="pos">bit数组中的对应位置</param>
        /// <param name="subPos">小数位</param>
        /// <returns>对应位置的bit元素</returns>
        public override bool GetBit(byte[] number, ref int pos, ref int subPos)
        {
            if (subPos < 0 && subPos > 7) throw new IndexOutOfRangeException();
            var tspos = 7 - subPos;
            var tpos = pos;
            var bit = GetBit(number[pos], ref tpos, ref tspos);
            subPos += 1;
            if (subPos > 7)
            {
                pos++;
                subPos = 0;
            }
            return bit;
        }

        /// <summary>
        ///     将byte数组中相应的位置转换为8个bit数字
        /// </summary>
        /// <param name="data">待转换的数组</param>
        /// <param name="pos">转换数字的位置</param>
        /// <returns>转换出的位数组</returns>
        public override bool[] GetBits(byte[] data, ref int pos)
        {
            var t = base.GetBits(data, ref pos);
            Array.Reverse(t);
            return t;
        }

        /// <summary>
        ///     设置对应数字中相应位置的bit的值
        /// </summary>
        /// <param name="number">byte数子</param>
        /// <param name="pos">设置的位置</param>
        /// <param name="subPos">设置的子位置</param>
        /// <param name="setBit">设置bit大小，true为1，false为0</param>
        /// <returns>设置是否成功</returns>
        public override bool SetBit(byte[] number, int pos, int subPos, bool setBit)
        {
            return base.SetBit(number, pos, 7 - subPos, setBit);
        }
    }
}