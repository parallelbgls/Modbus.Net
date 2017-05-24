using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     协议连接器接口
    /// </summary>
    /// <typeparam name="TParamIn">向Connector传入的数据类型</typeparam>
    /// <typeparam name="TParamOut">从Connector返回的数据类型</typeparam>
    public interface IProtocalLinker<TParamIn, TParamOut>
    {
        /// <summary>
        ///     通讯字符串
        /// </summary>
        string ConnectionToken { get; }

        /// <summary>
        ///     设备是否连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        bool Connect();

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
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        TParamOut SendReceive(TParamIn content);

        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        Task<TParamOut> SendReceiveAsync(TParamIn content);

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        TParamOut SendReceiveWithoutExtAndDec(TParamIn content);

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        Task<TParamOut> SendReceiveWithoutExtAndDecAsync(TParamIn content);

        /// <summary>
        ///     检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        bool? CheckRight(TParamOut content);

        /// <summary>
        ///     协议内容扩展，发送时根据需要扩展
        /// </summary>
        /// <param name="content">扩展前的基本协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        TParamIn BytesExtend(TParamIn content);

        /// <summary>
        ///     协议内容缩减，接收时根据需要缩减
        /// </summary>
        /// <param name="content">缩减前的完整协议内容</param>
        /// <returns>缩减后的协议内容</returns>
        TParamOut BytesDecact(TParamOut content);
    }
}