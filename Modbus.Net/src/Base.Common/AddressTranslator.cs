using System;

namespace Modbus.Net
{
    /// <summary>
    ///     地址定义类
    /// </summary>
    public class AddressDef
    {
        /// <summary>
        ///     地址区域的字符串描述
        /// </summary>
        public string AreaString { get; set; }

        /// <summary>
        ///     地址区域的数字描述
        /// </summary>
        public int Area { get; set; }

        /// <summary>
        ///     地址
        /// </summary>
        public int Address { get; set; }

        /// <summary>
        ///     子地址
        /// </summary>
        public int SubAddress { get; set; }
    }

    /// <summary>
    ///     地址区域数据定义类
    /// </summary>
    public class AreaOutputDef
    {
        /// <summary>
        ///     地址区域的编码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        ///     地址区域的单个地址占用的字节数
        /// </summary>
        public double AreaWidth { get; set; }
    }

    /// <summary>
    ///     地址翻译器
    /// </summary>
    public abstract class AddressTranslator
    {
        /// <summary>
        ///     地址转换
        /// </summary>
        /// <param name="address">格式化的地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <returns>翻译后的地址</returns>
        public abstract AddressDef AddressTranslate(string address, bool isRead);

        /// <summary>
        ///     获取区域中的单个地址占用的字节长度
        /// </summary>
        /// <param name="area">区域名称</param>
        /// <returns>字节长度</returns>
        public abstract double GetAreaByteLength(string area);
    }

    /// <summary>
    ///     基本的地址翻译器
    /// </summary>
    public class AddressTranslatorBase : AddressTranslator
    {
        /// <summary>
        ///     地址转换
        /// </summary>
        /// <param name="address">地址前地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <returns>Key为转换后的地址，Value为辅助码</returns>
        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            int num1, num2, num3;
            var split = address.Split(':');
            if (split.Length == 2)
            {
                if (int.TryParse(split[0], out num1) && int.TryParse(split[1], out num2))
                    return new AddressDef
                    {
                        Area = num1,
                        Address = num2
                    };
            }
            else if (split.Length == 3)
            {
                if (int.TryParse(split[0], out num1) && int.TryParse(split[1], out num2) &&
                    int.TryParse(split[3], out num3))
                    return new AddressDef
                    {
                        Area = num1,
                        Address = num2,
                        SubAddress = num3
                    };
            }
            throw new FormatException();
        }

        /// <summary>
        ///     获取区域中的单个地址占用的字节长度
        /// </summary>
        /// <param name="area">区域名称</param>
        /// <returns>字节长度</returns>
        public override double GetAreaByteLength(string area)
        {
            return 1;
        }
    }
}