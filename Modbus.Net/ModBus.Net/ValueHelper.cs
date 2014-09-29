using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    /// <summary>
    /// 值与字节数组之间转换的辅助类，这是一个Singleton类
    /// 作者：罗圣（Chris L.）
    /// </summary>
    public class ValueHelper
    {

        protected static bool _littleEndian = false;

        protected ValueHelper()
        {
        }

        /// <summary>
        /// 协议中的内容构造是否小端的，默认是大端构造协议。
        /// </summary>
        public static bool LittleEndian
        {
            get { return _littleEndian; }
            set
            {
                _littleEndian = value;
                //这里需要重点说明，因为.net默认是小端构造法，所以目标协议是大端的话反而需要调用小端构造协议，把小端反转为大端。
                _Instance = LittleEndian ? new ValueHelper() : new LittleEndianValueHelper();
            }
        }

        #region Factory

        protected static ValueHelper _Instance = null;

        public static ValueHelper Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = LittleEndian ? new ValueHelper() : new LittleEndianValueHelper();
                }
                return _Instance;
            }
        }

        #endregion

        public Byte[] GetBytes(byte value)
        {
            return new[] {value};
        }

        public virtual Byte[] GetBytes(short value)
        {
            return BitConverter.GetBytes(value);
        }

        public virtual Byte[] GetBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public virtual Byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value);
        }

        public virtual Byte[] GetBytes(ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        public virtual Byte[] GetBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        public virtual Byte[] GetBytes(ulong value)
        {
            return BitConverter.GetBytes(value);
        }

        public virtual Byte[] GetBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public virtual Byte[] GetBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }

        public virtual byte GetByte(byte[] data, ref int pos)
        {
            byte t = data[pos];
            pos += 1;
            return t;
        }

        public virtual short GetShort(byte[] data, ref int pos)
        {
            short t = BitConverter.ToInt16(data, pos);
            pos += 2;
            return t;
        }

        public virtual int GetInt(byte[] data, ref int pos)
        {
            int t = BitConverter.ToInt32(data, pos);
            pos += 4;
            return t;
        }

        public virtual long GetLong(byte[] data, ref int pos)
        {
            long t = BitConverter.ToInt64(data, pos);
            pos += 8;
            return t;
        }

        public virtual ushort GetUShort(byte[] data, ref int pos)
        {
            ushort t = BitConverter.ToUInt16(data, pos);
            pos += 2;
            return t;
        }

        public virtual uint GetUInt(byte[] data, ref int pos)
        {
            uint t = BitConverter.ToUInt32(data, pos);
            pos += 4;
            return t;
        }

        public virtual ulong GetULong(byte[] data, ref int pos)
        {
            ulong t = BitConverter.ToUInt64(data, pos);
            pos += 8;
            return t;
        }

        public virtual float GetFloat(byte[] data, ref int pos)
        {
            float t = BitConverter.ToSingle(data, pos);
            pos += 4;
            return t;
        }

        public virtual double GetDouble(byte[] data, ref int pos)
        {
            double t = BitConverter.ToDouble(data, pos);
            pos += 8;
            return t;
        }

        public virtual bool[] GetBits(byte[] data, ref int pos)
        {
            bool[] t = new bool[8];
            byte temp = data[pos];
            for (int i = 0; i < 8; i++)
            {
                t[i] = temp%2 > 0;
                temp /= 2;
            }
            return t;
        }

        public byte[] ObjectArrayToByteArray(object[] contents)
        {
            bool b = false;
            //先查找传入的结构中有没有数组，有的话将其打开
            var newContentsList = new List<object>();
            foreach (object content in contents)
            {
                string t = content.GetType().ToString();
                if (t.Substring(t.Length - 2, 2) == "[]")
                {
                    b = true;
                    IEnumerable<object> contentArray =
                        ArrayList.Adapter((Array) content).ToArray(typeof (object)).OfType<object>();
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
            bool lastIsBool = false;
            byte boolToByteTemp = 0;
            int boolToByteCount = 0;
            foreach (object content in contents)
            {
                string t = content.GetType().ToString();
                if (t == "System.Boolean")
                {
                    if (boolToByteCount >= 8)
                    {
                        translateTarget.Add(boolToByteTemp);
                        boolToByteCount = 0;
                        boolToByteTemp = 0;
                    }
                    lastIsBool = true;
                    if (_littleEndian)
                    {
                        boolToByteTemp += (byte) ((bool) content ? Math.Pow(2, boolToByteCount) : 0);
                    }
                    else
                    {
                        boolToByteTemp = (byte) (boolToByteTemp*2 + ((bool) content ? 1 : 0));
                    }
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
                                translateTarget.AddRange(Instance.GetBytes((short)content));
                                break;
                            }
                        case "System.Int32":
                            {
                                translateTarget.AddRange(Instance.GetBytes((int)content));
                                break;
                            }
                        case "System.Int64":
                            {
                                translateTarget.AddRange(Instance.GetBytes((long)content));
                                break;
                            }
                        case "System.UInt16":
                            {
                                translateTarget.AddRange(Instance.GetBytes((ushort)content));
                                break;
                            }
                        case "System.UInt32":
                            {
                                translateTarget.AddRange(Instance.GetBytes((uint)content));
                                break;
                            }
                        case "System.UInt64":
                            {
                                translateTarget.AddRange(Instance.GetBytes((ulong)content));
                                break;
                            }
                        case "System.Single":
                            {
                                translateTarget.AddRange(Instance.GetBytes((float)content));
                                break;
                            }
                        case "System.Double":
                            {
                                translateTarget.AddRange(Instance.GetBytes((double)content));
                                break;
                            }
                        case "System.Byte":
                            {
                                translateTarget.AddRange(Instance.GetBytes((byte)content));
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("没有实现除整数以外的其它转换方式");
                            }
                    }
                }        
            }
            //最后把队列转换为数组
            return translateTarget.ToArray();
        }

        public object[] ByteArrayToObjectArray(byte[] contents,
            IEnumerable<KeyValuePair<Type, int>> translateTypeAndCount)
        {
            List<object> translation = new List<object>();
            int count = 0;
            foreach (var translateUnit in translateTypeAndCount)
            {
                for (int i = 0; i < translateUnit.Value; i++)
                {
                    if (count >= contents.Length) break;
                    try
                    {
                        switch (translateUnit.Key.ToString())
                        {
                            case "System.Int16":
                            {
                                short value = Instance.GetShort(contents, ref count);
                                translation.Add(value);
                                break;
                            }
                            case "System.Int32":
                            {
                                int value = Instance.GetInt(contents, ref count);
                                translation.Add(value);
                                break;
                            }
                            case "System.Int64":
                            {
                                long value = Instance.GetLong(contents, ref count);
                                translation.Add(value);
                                break;
                            }
                            case "System.UInt16":
                            {
                                ushort value = Instance.GetUShort(contents, ref count);
                                translation.Add(value);
                                break;
                            }
                            case "System.UInt32":
                            {
                                uint value = Instance.GetUInt(contents, ref count);
                                translation.Add(value);
                                break;
                            }
                            case "System.UInt64":
                            {
                                ulong value = Instance.GetULong(contents, ref count);
                                translation.Add(value);
                                break;
                            }
                            case "System.Single":
                            {
                                float value = Instance.GetFloat(contents, ref count);
                                translation.Add(value);
                                break;
                            }
                            case "System.Double":
                            {
                                double value = Instance.GetDouble(contents, ref count);
                                translation.Add(value);
                                break;
                            }
                            case "System.Byte":
                            {
                                byte value = Instance.GetByte(contents, ref count);
                                translation.Add(value);
                                break;
                            }
                            case "System.Boolean":
                            {
                                bool[] value = Instance.GetBits(contents, ref count);
                                for (int j = 0; j < value.Length; j++)
                                {
                                    translation.Add(value[j]);
                                }
                                break;
                            }
                            default:
                            {
                                throw new NotImplementedException("没有实现除整数以外的其它转换方式");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        count = contents.Length;
                    }
                }
            }
            return translation.ToArray();
        }

        public bool GetBit(ushort number, int pos)
        {           
            if (pos < 0 && pos > 15) throw new IndexOutOfRangeException();
            int ans = number % 2;
            int i = 15;
            while (i >= pos)
            {
                ans = number%2;
                number /= 2;
                i--;
            }
            return ans > 0;
        }

        public ushort SetBit(ushort number, int pos, bool setBit)
        {
            int creation = 0;
            if (setBit)
            {
                for (int i = 0; i < 16; i++)
                {
                    creation *= 2;
                    if (i == pos) creation++;
                }
                return (ushort) (number | creation);
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    creation *= 2;
                    if (i != pos) creation++;
                }
                return (ushort) (number & creation);
            }
        }
    }

    internal class LittleEndianValueHelper : ValueHelper
    {
        public override Byte[] GetBytes(short value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        public override Byte[] GetBytes(int value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        public override Byte[] GetBytes(long value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        public override Byte[] GetBytes(ushort value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        public override Byte[] GetBytes(uint value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        public override Byte[] GetBytes(ulong value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        public override Byte[] GetBytes(float value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        public override Byte[] GetBytes(double value)
        {
            return Reverse(BitConverter.GetBytes(value));
        }

        public override short GetShort(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 2);
            short t = BitConverter.ToInt16(data, pos);
            pos += 2;
            return t;
        }

        public override int GetInt(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            int t = BitConverter.ToInt32(data, pos);
            pos += 4;
            return t;
        }

        public override long GetLong(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            long t = BitConverter.ToInt64(data, pos);
            pos += 8;
            return t;
        }

        public override ushort GetUShort(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 2);
            ushort t = BitConverter.ToUInt16(data, pos);
            pos += 2;
            return t;
        }

        public override uint GetUInt(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            uint t = BitConverter.ToUInt32(data, pos);
            pos += 4;
            return t;
        }

        public override ulong GetULong(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            ulong t = BitConverter.ToUInt64(data, pos);
            pos += 8;
            return t;
        }

        public override float GetFloat(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            float t = BitConverter.ToSingle(data, pos);
            pos += 4;
            return t;
        }

        public override double GetDouble(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            double t = BitConverter.ToDouble(data, pos);
            pos += 8;
            return t;
        }

        public override bool[] GetBits(byte[] data, ref int pos)
        {
            bool[] t = new bool[8];
            byte temp = data[pos];
            for (int i = 0; i < 8; i++)
            {
                t[8 - i] = temp%2 > 0;
                temp /= 2;
            }
            return t;
        }

        private Byte[] Reverse(Byte[] data)
        {
            Array.Reverse(data);
            return data;
        }
    }
}