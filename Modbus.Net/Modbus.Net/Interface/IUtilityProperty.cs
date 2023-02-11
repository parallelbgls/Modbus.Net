using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Interface
{
    /// <summary>
    ///     Api入口的抽象
    /// </summary>
    public interface IUtilityProperty
    {
        /// <summary>
        ///     协议是否遵循小端格式
        /// </summary>
        Endian Endian { get; }

        /// <summary>
        ///     设备是否已经连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     标识设备的连接关键字
        /// </summary>
        string ConnectionToken { get; }

        /// <summary>
        ///     地址翻译器
        /// </summary>
        AddressTranslator AddressTranslator { get; set; }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        ///     断开设备
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        bool Disconnect();

        /// <summary>
        ///     返回Utility的方法集合
        /// </summary>
        /// <typeparam name="TUtilityMethod">Utility方法集合类型</typeparam>
        /// <returns>Utility方法集合</returns>
        TUtilityMethod GetUtilityMethods<TUtilityMethod>() where TUtilityMethod : class, IUtilityMethod;
    }
}
