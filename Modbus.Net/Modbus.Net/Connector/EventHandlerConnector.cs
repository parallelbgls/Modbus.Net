using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <inheritdoc />
    public abstract partial class EventHandlerConnector : EventHandlerConnector<byte[], byte[]>
    {
        private static readonly ILogger<EventHandlerConnector> logger = LogProvider.CreateLogger<EventHandlerConnector>();

        /// <inheridoc />
        public override bool IsSharable => true;

        /// <inheridoc />
        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            ctx.Flush();
        }

        /// <inheridoc />
        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            logger.LogError(e, e.ToString());
            ctx.CloseAsync();
        }
    }

    /// <summary>
    ///     基础的协议连接类
    /// </summary>
    public abstract class EventHandlerConnector<TParamIn, TParamOut> : ChannelHandlerAdapter, IConnectorWithController<TParamIn, TParamOut> where TParamIn : class
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