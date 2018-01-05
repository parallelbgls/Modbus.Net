using System.Globalization;
using System.Text;

namespace Modbus.Net
{
    /// <summary>
    ///     串口辅助类
    /// </summary>
    public static class SerialPortHelper
    {
        /// <summary>
        ///     字符数组转字符串16进制
        /// </summary>
        /// <param name="inBytes"> 二进制字节 </param>
        /// <returns>类似"01 02 0F" </returns>
        public static string ByteToString(this byte[] inBytes)
        {
            var stringOut = "";
            foreach (var inByte in inBytes)
                stringOut = stringOut + $"{inByte:X2}" + " ";

            return stringOut.Trim();
        }

        /// <summary>
        ///     strhex 转字节数组
        /// </summary>
        /// <param name="inString">类似"01 02 0F" 用空格分开的  </param>
        /// <returns> </returns>
        public static byte[] StringToByte(this string inString)
        {
            var byteStrings = inString.Split(" ".ToCharArray());
            var byteOut = new byte[byteStrings.Length];
            for (var i = 0; i <= byteStrings.Length - 1; i++)
                byteOut[i] = byte.Parse(byteStrings[i], NumberStyles.HexNumber);
            return byteOut;
        }

        /// <summary>
        ///     strhex 转字节数组
        /// </summary>
        /// <param name="inString">类似"01 02 0F" 中间无空格 </param>
        /// <returns> </returns>
        public static byte[] StringToByte_2(this string inString)
        {
            inString = inString.Replace(" ", "");

            var byteStrings = new string[inString.Length / 2];
            var j = 0;
            for (var i = 0; i < byteStrings.Length; i++)
            {
                byteStrings[i] = inString.Substring(j, 2);
                j += 2;
            }

            var byteOut = new byte[byteStrings.Length];
            for (var i = 0; i <= byteStrings.Length - 1; i++)
                byteOut[i] = byte.Parse(byteStrings[i], NumberStyles.HexNumber);

            return byteOut;
        }

        /// <summary>
        ///     字符串 转16进制字符串
        /// </summary>
        /// <param name="inString">unico </param>
        /// <returns>类似“01 0f” </returns>
        public static string Str_To_0X(this string inString)
        {
            return ByteToString(Encoding.Default.GetBytes(inString));
        }
    }
}
