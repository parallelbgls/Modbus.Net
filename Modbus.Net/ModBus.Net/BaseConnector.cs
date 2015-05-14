using System;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public abstract class BaseConnector
    {
        public abstract string ConnectionToken { get; }
        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        public abstract bool IsConnected { get; }
        /// <summary>
        /// 连接PLC
        /// </summary>
        /// <returns></returns>
        public abstract bool Connect();
        /// <summary>
        /// 连接PLC，异步
        /// </summary>
        /// /// <returns></returns>
        public abstract Task<bool> ConnectAsync();
        /// <summary>
        /// 断开PLC
        /// </summary>
        /// <returns></returns>
        public abstract bool Disconnect();
        /// <summary>
        /// 无返回发送数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool SendMsgWithoutReturn(byte[] message);
        /// <summary>
        /// 无返回发送数据，异步
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task<bool> SendMsgWithoutReturnAsync(byte[] message);
        /// <summary>
        /// 带返回发送数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract byte[] SendMsg(byte[] message);
        /// <summary>
        /// 带返回发送数据，异步
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task<byte[]> SendMsgAsync(byte[] message);
    }
}
