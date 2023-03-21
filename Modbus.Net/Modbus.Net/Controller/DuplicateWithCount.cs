using System;
using System.Collections.Generic;

namespace Modbus.Net
{
    /// <summary>
    ///     按照长度断包的函数
    /// </summary>
    public static class DuplicateWithCount
    {
        /// <summary>
        ///     计算切分包的长度
        /// </summary>
        /// <param name="receiveMessage">收到的报文信息</param>
        /// <param name="packageCountPositions">收到的断包长度查询位置</param>
        /// <param name="otherCount">除指示的长度外其它位置的长度</param>
        /// <returns>切分后的报文信息</returns>
        private static int CalculateLength(byte[] receiveMessage, ICollection<int> packageCountPositions, int otherCount)
        {
            var ans = 0;
            foreach (var position in packageCountPositions)
            {
                if (position >= receiveMessage.Length) return -1;
                ans = ans * 256 + receiveMessage[position];
            }
            return ans + otherCount;
        }

        /// <summary>
        ///     获取长度函数
        /// </summary>
        /// <param name="packageCountPositions">断包长度的位置信息</param>
        /// <param name="otherCount">除指示的长度外其它位置的长度</param>
        /// <returns>断包函数</returns>
        public static Func<byte[], int> GetDuplcateFunc(ICollection<int> packageCountPositions, int otherCount)
        {
            return receiveMessage => CalculateLength(receiveMessage, packageCountPositions, otherCount);
        }
    }
}
