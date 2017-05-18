using System;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc地址解析器
    /// </summary>
    public class AddressTranslatorOpc : AddressTranslator
    {
        /// <summary>
        ///     地址转换
        /// </summary>
        /// <param name="address">格式化的地址</param>
        /// <param name="isRead">是否为读取，是为读取，否为写入</param>
        /// <returns>翻译后的地址</returns>
        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            throw new NotImplementedException();
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