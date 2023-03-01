using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <inheritdoc />
    public abstract class BaseConnector : BaseConnector<byte[], byte[]>
    {
        private static readonly ILogger<BaseConnector> logger = LogProvider.CreateLogger<BaseConnector>();

        /// <summary>
        ///     发送锁
        /// </summary>
        protected abstract AsyncLock Lock { get; }

        /// <summary>
        ///     是否为全双工
        /// </summary>
        public bool IsFullDuplex { get; }

        /// <summary>
        ///     发送超时时间
        /// </summary>
        protected abstract int TimeoutTime { get; set; }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="timeoutTime">发送超时时间</param>
        /// <param name="isFullDuplex">是否为全双工</param>
        protected BaseConnector(int timeoutTime = 10000, bool isFullDuplex = true)
        {
            IsFullDuplex = isFullDuplex;
            TimeoutTime = timeoutTime;
        }

        /// <inheritdoc />
        public override async Task<byte[]> SendMsgAsync(byte[] message)
        {
            var ans = await SendMsgInner(message);
            if (ans == null) return new byte[0];
            return ans.ReceiveMessage;
        }

        /// <summary>
        ///     发送内部
        /// </summary>
        /// <param name="message">发送的信息</param>
        /// <returns>发送信息的定义</returns>
        protected async Task<MessageWaitingDef> SendMsgInner(byte[] message)
        {
            IDisposable asyncLock = null;
            try
            {
                var messageSendingdef = Controller.AddMessage(message);
                if (messageSendingdef != null)
                {
                    if (!IsFullDuplex)
                    {
                        asyncLock = await Lock.LockAsync();
                    }
                    var success = messageSendingdef.SendMutex.WaitOne(TimeoutTime);
                    if (success)
                    {
                        await SendMsgWithoutConfirm(message);
                        success = messageSendingdef.ReceiveMutex.WaitOne(TimeoutTime);
                        if (success)
                        {
                            return messageSendingdef;
                        }
                    }
                    Controller.ForceRemoveWaitingMessage(messageSendingdef);
                }
                logger.LogInformation("Message is waiting in {0}. Cancel!", ConnectionToken);
                return null;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Connector {0} Send Error.", ConnectionToken);
                return null;
            }
            finally
            {
                asyncLock?.Dispose();
            }
        }
    }

    /// <summary>
    ///     基础的协议连接类
    /// </summary>
    public abstract class BaseConnector<TParamIn, TParamOut> : IConnector<TParamIn, TParamOut> where TParamIn : class
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