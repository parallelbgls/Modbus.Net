using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     按照长度断包的函数
    /// </summary>
    public static class DuplicateWithCount
    {
        /// <summary>
        ///     切分包
        /// </summary>
        /// <param name="receiveMessage">收到的报文信息</param>
        /// <param name="packageCountPositions">收到的断包长度查询位置</param>
        /// <returns>切分后的报文信息</returns>
        private static ICollection<byte[]> DuplicateMessages(byte[] receiveMessage, ICollection<int> packageCountPositions, int otherCount)
        {
            if (packageCountPositions == null)
                return new List<byte[]> { receiveMessage };
            var ans = new List<byte[]>();
            var pos = 0;
            while (pos < receiveMessage.Length)
            {
                try
                {
                    var length = 0;
                    foreach (var countPos in packageCountPositions)
                    {
                        length = length * 256 + receiveMessage[pos + countPos];
                    }
                    length += otherCount;
                    if (pos + length > receiveMessage.Length) break;
                    byte[] currentPackage = new byte[length];
                    Array.Copy(receiveMessage, pos, currentPackage, 0, length);
                    ans.Add(currentPackage);
                    pos += length;
                }
                catch (Exception)
                {
                    break;
                }
            }
            return ans;
        }

        /// <summary>
        ///     获取按照长度断包的函数
        /// </summary>
        /// <param name="packageCountPositions">断包长度的位置信息</param>
        /// <returns>断包函数</returns>
        public static Func<byte[], ICollection<byte[]>> GetDuplcateFunc(ICollection<int> packageCountPositions, int otherCount)
        {
            return receiveMessage => DuplicateMessages(receiveMessage, packageCountPositions, otherCount);
        }
    }
}
