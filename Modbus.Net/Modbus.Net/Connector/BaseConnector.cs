using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <inheritdoc />
    public abstract partial class BaseConnector : BaseConnector<byte[], byte[]>
    {
        private static readonly ILogger<EventHandlerConnector> logger = LogProvider.CreateLogger<EventHandlerConnector>();
    }

    /// <summary>
    ///     基础的协议连接类
    /// </summary>
    public abstract class BaseConnector<TParamIn, TParamOut> : IConnectorWithController<TParamIn, TParamOut> where TParamIn : class
    {
        /// <summary>
        ///     数据返回代理参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate MessageReturnCallbackArgs<TParamIn> MessageReturnDelegate(object sender, MessageReturnArgs<TParamOut> args);

        /// <summary>
        ///     数据返回代理
        /// </summary>
        public event MessageReturnDelegate MessageReturn;

        /// <inheritdoc />
        public void AddController(IController controller)
        {
            Controller = controller;
        }

        /// <summary>
        ///     传输控制器
        /// </summary>
        protected virtual IController Controller { get; set; }

        /// <inheritdoc />
        public abstract string ConnectionToken { get; }

        /// <inheritdoc />
        public abstract bool IsConnected { get; }

        /// <inheritdoc />
        public abstract Task<bool> ConnectAsync();

        /// <inheritdoc />
        public abstract bool Disconnect();

        /// <inheritdoc />
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

        /// <summary>
        ///     数据返回代理函数
        /// </summary>
        /// <param name="receiveMessage"></param>
        /// <returns></returns>
        protected TParamIn InvokeReturnMessage(TParamOut receiveMessage)
        {
            return MessageReturn?.Invoke(this, new MessageReturnArgs<TParamOut> { ReturnMessage = receiveMessage })?.SendMessage;
        }
    }
}