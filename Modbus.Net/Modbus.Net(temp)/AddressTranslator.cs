using System;
using System.Collections.Generic;

namespace Modbus.Net
{
    /// <summary>
    /// 地址翻译器
    /// </summary>
    public abstract class AddressTranslator
    {
        /// <summary>
        /// 地址转换
        /// </summary>
        /// <param name="address">地址前地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <returns>Key为转换后的地址，Value为辅助码</returns>
        public abstract KeyValuePair<int,int> AddressTranslate(string address, bool isRead);
    }

    /// <summary>
    /// 基本的地址翻译器
    /// </summary>
    public class AddressTranslatorBase : AddressTranslator
    {
        public override KeyValuePair<int, int> AddressTranslate(string address, bool isRead)
        {
            int num1,num2;
            string[] split = address.Split(':');
            if (split.Length != 2) throw new FormatException();
            if (int.TryParse(split[0], out num1) && int.TryParse(split[1], out num2))
            {
                return new KeyValuePair<int, int>(num2, num1);
            }
            throw new FormatException();
        }
    }
}