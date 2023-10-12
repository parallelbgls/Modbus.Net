using System;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     协议接收器接口
    /// </summary>
    /// <typeparam name="TParamIn">从Receiver传出的数据类型</typeparam>
    /// <typeparam name="TParamOut">向Receiver传入的数据类型</typeparam>
    public interface IProtocolReceiver<TParamIn, TParamOut>
    {
        /// <summary>
        ///     转发事件
        /// </summary>
        Func<TParamOut, TParamIn> DispatchEvent { get; }

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
        Task<bool> ConnectAsync();

        /// <summary>
        ///     断开设备
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        bool Disconnect();

        /// <summary>
        ///     接收并发送数据
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>发送协议的内容</returns>
        TParamIn ReceiveSend(TParamOut content);

        /// <summary>
        ///     接收并发送数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        TParamIn ReceiveSendWithoutExtAndDec(TParamOut content);

        /// <summary>
        ///     检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        bool? CheckRight(TParamOut content);
    }
}