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
        //protected delegate MessageReturnCallbackArgs MessageReturnDelegate(object sender, MessageReturnArgs args);

        //protected event MessageReturnDelegate MessageReturn;

        /// <summary>
        ///     增加传输控制器
        /// </summary>
        /// <param name="controller">传输控制器</param>
        public void AddController(IController controller)
        {
            Controller = controller;
        }

        /// <summary>
        ///     传输控制器
        /// </summary>
        protected IController Controller { get; set; }

        /// <summary>
        ///     标识Connector的连接关键字
        /// </summary>
        public abstract string ConnectionToken { get; }

        /// <summary>
        ///     是否处于连接状态
        /// </summary>
        public abstract bool IsConnected { get; }

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
        public abstract Task<TParamOut> SendMsgAsync(TParamIn message);

        /// <summary>
        ///     发送数据，不确认
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        protected abstract Task SendMsgWithoutConfirm(TParamIn message);

        /// <summary>
        ///     接收消息单独线程开启
        /// </summary>
        protected abstract void ReceiveMsgThreadStart();

        /// <summary>
        ///     接收消息单独线程停止
        /// </summary>
        protected abstract void ReceiveMsgThreadStop();
    }

    /*public class MessageReturnArgs
    {
        public byte[] ReturnMessage { get; set; }

        public string MessageKey { get; set; }
    }

    public class MessageReturnCallbackArgs
    {
        public bool ShouldLockSender { get; set; } = false;

        public bool ShouldReleaseSender { get; set; } = false;
    }*/
}