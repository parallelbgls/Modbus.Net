using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Serilog;

namespace Modbus.Net
{
    /// <inheritdoc />
    public abstract class BaseConnector : BaseConnector<byte[], byte[]>
    {
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
            var ans = await SendMsgCtrl(message);
            if (ans == null) return new byte[0];
            return ans.ReceiveMessage;
        }

        /// <summary>
        ///     发送主控
        /// </summary>
        /// <param name="message">发送的信息</param>
        /// <returns>等待信息的定义</returns>
        protected async Task<MessageWaitingDef> SendMsgCtrl(byte[] message)
        {
            MessageWaitingDef ans;
            if (!IsFullDuplex)
            {
                using (await Lock.LockAsync())
                {
                    ans = await SendMsgInner(message);
                }
            }
            else
            {
                ans = await SendMsgInner(message);
            }
            return ans;
        }

        /// <summary>
        ///     发送内部
        /// </summary>
        /// <param name="message">发送的信息</param>
        /// <returns>发送信息的定义</returns>
        protected async Task<MessageWaitingDef> SendMsgInner(byte[] message)
        {
            try
            {
                var messageSendingdef = Controller.AddMessage(message);
                if (messageSendingdef != null)
                {
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
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e, "Connector {0} Send Error.", ConnectionToken);
                return null;
            }
            
        }
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