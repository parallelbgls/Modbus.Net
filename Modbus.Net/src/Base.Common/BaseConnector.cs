using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     基础的协议连接类
    /// </summary>
    public abstract class BaseConnector : BaseConnector<byte[], byte[]>
    {
    }

    /// <summary>
    ///     基础的协议连接类
    /// </summary>
    public abstract class BaseConnector<TParamIn, TParamOut> : IConnector<TParamIn, TParamOut>
    {
        /// <summary>
        ///     标识Connector的连接关键字
        /// </summary>
        public abstract string ConnectionToken { get; }

        /// <summary>
        ///     是否处于连接状态
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        ///     连接PLC
        /// </summary>
        /// <returns>是否连接成功</returns>
        public abstract bool Connect();

        /// <summary>
        ///     连接PLC，异步
        /// </summary>
        /// <returns>是否连接成功</returns>
        public abstract Task<bool> ConnectAsync();

        /// <summary>
        ///     断开PLC
        /// </summary>
        /// <returns>是否断开成功</returns>
        public abstract bool Disconnect();

        /// <summary>
        ///     带返回发送数据
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public abstract TParamOut SendMsg(TParamIn message);

        /// <summary>
        ///     带返回发送数据
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public abstract Task<TParamOut> SendMsgAsync(TParamIn message);
    }
}