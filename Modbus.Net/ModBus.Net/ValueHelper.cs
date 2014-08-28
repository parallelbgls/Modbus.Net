using System;

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

        internal static ValueHelper Instance
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

        public byte GetByte(byte[] data, ref int pos)
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
            ulong t = BitConverter.ToUInt64(data, 0);
            pos += 8;
            return t;
        }

        public virtual float GetFloat(byte[] data, ref int pos)
        {
            float t = BitConverter.ToSingle(data, 0);
            pos += 4;
            return t;
        }

        public virtual double GetDouble(byte[] data, ref int pos)
        {
            double t = BitConverter.ToDouble(data, 0);
            pos += 8;
            return t;
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
            ulong t = BitConverter.ToUInt64(data, 0);
            pos += 8;
            return t;
        }

        public override float GetFloat(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            float t = BitConverter.ToSingle(data, 0);
            pos += 4;
            return t;
        }

        public override double GetDouble(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            double t = BitConverter.ToDouble(data, 0);
            pos += 8;
            return t;
        }

        private Byte[] Reverse(Byte[] data)
        {
            Array.Reverse(data);
            return data;
        }
    }
}