using System;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     没有Id的设备属性
    /// </summary>
    public interface IMachinePropertyWithoutKey
    {
        /// <summary>
        ///     工程名
        /// </summary>
        string ProjectName { get; set; }

        /// <summary>
        ///     设备名
        /// </summary>
        string MachineName { get; set; }

        /// <summary>
        ///     标识设备的连接关键字
        /// </summary>
        string ConnectionToken { get; }

        /// <summary>
        ///     是否处于连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     是否保持连接
        /// </summary>
        bool KeepConnect { get; set; }

        /// <summary>
        ///     设备的连接器
        /// </summary>
        IUtilityProperty BaseUtility { get; }

        /// <summary>
        ///     获取设备的方法集合
        /// </summary>
        /// <typeparam name="TMachineMethod">方法集合的类型</typeparam>
        /// <returns>设备的方法集合</returns>
        TMachineMethod GetMachineMethods<TMachineMethod>() where TMachineMethod : class, IMachineMethod;

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        ///     断开设备
        /// </summary>
        /// <returns>是否断开成功</returns>
        bool Disconnect();

        /// <summary>
        ///     获取设备的Id的字符串
        /// </summary>
        /// <returns></returns>
        string GetMachineIdString();
    }

    /// <summary>
    ///     设备属性的抽象
    /// </summary>
    public interface IMachineProperty<TKey> : IMachinePropertyWithoutKey where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     Id
        /// </summary>
        TKey Id { get; set; }
    }
}
