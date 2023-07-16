using System;

namespace Modbus.Net
{
    public partial class Endian
    {
        public const int BigEndian3412 = 10;
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
    }
}