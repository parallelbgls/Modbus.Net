using System;

namespace Modbus.Net
{
    public partial class Endian
    {
        public const int BigEndian3412 = 10;

        public const int LittleEndian3412 = 11;
    }

    public class BigEndian3412ValueHelper : BigEndianLsbValueHelper
    {
        private static BigEndian3412ValueHelper _bigEndian3412Instance;

        /// <summary>
        ///     构造器
        /// </summary>
        protected BigEndian3412ValueHelper()
        {
        }

        /// <summary>
        ///     覆写的实例获取
        /// </summary>
        protected override ValueHelper _Instance => _bigEndian3412Instance;

        /// <summary>
        ///     是否为大端
        /// </summary>
        protected new bool LittleEndian => false;

        protected new bool LittleEndianBit => false;

        /// <summary>
        ///     覆盖的获取实例的方法
        /// </summary>
        public new static BigEndian3412ValueHelper Instance
            => _bigEndian3412Instance ?? (_bigEndian3412Instance = new BigEndian3412ValueHelper());

        public override int GetInt(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            var t = BitConverter.ToInt32(data, pos);
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            Array.Reverse(data, pos, 4);
            pos += 4;
            return t;
        }

        public override uint GetUInt(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            var t = BitConverter.ToUInt32(data, pos);
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            Array.Reverse(data, pos, 4);
            pos += 4;
            return t;
        }

        public override long GetLong(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            var t = BitConverter.ToInt64(data, pos);
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            Array.Reverse(data, pos, 8);
            pos += 8;
            return t;
        }

        public override ulong GetULong(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            var t = BitConverter.ToUInt64(data, pos);
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            Array.Reverse(data, pos, 8);
            pos += 8;
            return t;
        } 

        public override float GetFloat(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 4);
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            var t = BitConverter.ToSingle(data, pos);
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            Array.Reverse(data, pos, 4);
            pos += 4;
            return t;
        }

        public override double GetDouble(byte[] data, ref int pos)
        {
            Array.Reverse(data, pos, 8);
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            var t = BitConverter.ToDouble(data, pos);
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            Array.Reverse(data, pos, 8);
            pos += 8;
            return t;
        }
    }

    public class LittleEndian3412ValueHelper : LittleEndianLsbValueHelper
    {
        private static LittleEndian3412ValueHelper _littleEndian3412Instance;

        /// <summary>
        ///     构造器
        /// </summary>
        protected LittleEndian3412ValueHelper()
        {
        }

        /// <summary>
        ///     覆写的实例获取
        /// </summary>
        protected override ValueHelper _Instance => _littleEndian3412Instance;

        /// <summary>
        ///     是否为大端
        /// </summary>
        protected new bool LittleEndian => true;

        protected new bool LittleEndianBit => false;

        /// <summary>
        ///     覆盖的获取实例的方法
        /// </summary>
        public new static LittleEndian3412ValueHelper Instance
            => _littleEndian3412Instance ?? (_littleEndian3412Instance = new LittleEndian3412ValueHelper());

        public override int GetInt(byte[] data, ref int pos)
        {
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            var t = BitConverter.ToInt32(data, pos);
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            pos += 4;
            return t;
        }

        public override uint GetUInt(byte[] data, ref int pos)
        {
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            var t = BitConverter.ToUInt32(data, pos);
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            pos += 4;
            return t;
        }

        public override long GetLong(byte[] data, ref int pos)
        {
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            var t = BitConverter.ToInt64(data, pos);
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            pos += 8;
            return t;
        }

        public override ulong GetULong(byte[] data, ref int pos)
        {
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            var t = BitConverter.ToUInt64(data, pos);
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            pos += 8;
            return t;
        }

        public override float GetFloat(byte[] data, ref int pos)
        {
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            var t = BitConverter.ToSingle(data, pos);
            temp = data[pos]; data[pos] = data[pos + 2]; data[pos + 2] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 3]; data[pos + 3] = temp;
            pos += 4;
            return t;
        }

        public override double GetDouble(byte[] data, ref int pos)
        {
            byte temp;
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            var t = BitConverter.ToDouble(data, pos);
            temp = data[pos]; data[pos] = data[pos + 6]; data[pos + 6] = temp;
            temp = data[pos + 1]; data[pos + 1] = data[pos + 7]; data[pos + 7] = temp;
            temp = data[pos + 2]; data[pos + 2] = data[pos + 4]; data[pos + 4] = temp;
            temp = data[pos + 3]; data[pos + 3] = data[pos + 5]; data[pos + 5] = temp;
            pos += 8;
            return t;
        }
    }
}