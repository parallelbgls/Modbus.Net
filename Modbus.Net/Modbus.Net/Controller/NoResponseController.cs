using Microsoft.Extensions.Logging;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     控制器基类
    /// </summary>
    public class NoResponseController : IController
    {
        private static readonly ILogger<NoResponseController> logger = LogProvider.CreateLogger<NoResponseController>();

        /// <summary>
        ///     等待的消息队列
        /// </summary>
        protected List<MessageWaitingDef> WaitingMessages { get; set; }

        /// <summary>
        ///     消息维护线程
        /// </summary>
        protected Task SendingThread { get; set; }

        /// <summary>
        ///     间隔时间
        /// </summary>
        public int AcquireTime { get; }

        /// <summary>
        ///     消息维护线程是否在运行
        /// </summary>
        public virtual bool IsSending => SendingThread != null;

        private MessageWaitingDef _currentSendingPos;

        private CancellationTokenSource _sendingThreadCancel;

        /// <summary>
        ///     构造器
        /// </summary>
        public NoResponseController(int acquireTime)
        {
            WaitingMessages = new List<MessageWaitingDef>();
            AcquireTime = acquireTime;
        }

        /// <inheritdoc />
        public MessageWaitingDef AddMessage(byte[] sendMessage)
        {
            var def = new MessageWaitingDef
            {
                Key = GetKeyFromMessage(sendMessage)?.Item1,
                SendMessage = sendMessage,
                SendMutex = new AutoResetEvent(false),
                ReceiveMutex = new AutoResetEvent(false)
            };
            if (AddMessageToList(def))
            {
                return def;
            }
            return null;
        }

        /// <summary>
        ///     发送消息的实际内部方法
        /// </summary>
        protected void SendingMessageControlInner(CancellationToken token)
        {
            while (true)
            {
                if (AcquireTime > 0)
                {
                    Thread.Sleep(AcquireTime);
                }
                lock (WaitingMessages)
                {
                    try
                    {
                        if (_currentSendingPos == null)
                        {
                            if (WaitingMessages.Count > 0)
                            {
                                _currentSendingPos = WaitingMessages.First();
                                _currentSendingPos.SendMutex.Set();
                                _currentSendingPos.ReceiveMessage = new byte[0];
                                _currentSendingPos.ReceiveMutex.Set();
                                ForceRemoveWaitingMessage(_currentSendingPos);
                            }
                        }
                        else
                        {
                            if (WaitingMessages.Count <= 0)
                            {
                                _currentSendingPos = null;
                            }
                            else if (WaitingMessages.IndexOf(_currentSendingPos) == -1)
                            {
                                _currentSendingPos = WaitingMessages.First();
                                _currentSendingPos.SendMutex.Set();
                                _currentSendingPos.ReceiveMessage = new byte[0];
                                _currentSendingPos.ReceiveMutex.Set();
                                ForceRemoveWaitingMessage(_currentSendingPos);
                            }
                        }
                    }
                    catch (ObjectDisposedException e)
                    {
                        logger.LogError(e, "Controller _currentSendingPos disposed");
                        _currentSendingPos = null;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Controller throws exception");
                        SendStop();
                    }
                }
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        /// <inheritdoc />
        public virtual void SendStop()
        {
            Clear();
            _sendingThreadCancel?.Cancel();
            if (SendingThread != null)
            {
                while (!SendingThread.IsCanceled)
                {
                    Thread.Sleep(10);
                }
                SendingThread.Dispose();
                SendingThread = null;
            }
            Clear();
        }

        /// <inheritdoc />
        public virtual async void SendStart()
        {
            if (!IsSending)
            {
                _sendingThreadCancel = new CancellationTokenSource();
                SendingThread = Task.Run(() => SendingMessageControlInner(_sendingThreadCancel.Token), _sendingThreadCancel.Token);
                try
                {
                    await SendingThread;
                }
                catch (OperationCanceledException)
                { }
                finally
                {
                    _sendingThreadCancel.Dispose();
                    _sendingThreadCancel = null;
                }
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (WaitingMessages != null)
            {
                lock (WaitingMessages)
                {
                    WaitingMessages.Clear();
                }
            }
        }

        /// <summary>
        ///     将信息添加到队列
        /// </summary>
        /// <param name="def">需要添加的信息信息</param>
        protected virtual bool AddMessageToList(MessageWaitingDef def)
        {
            var ans = false;
            lock (WaitingMessages)
            {
                if (WaitingMessages.FirstOrDefault(p => p.Key == def.Key) == null || def.Key == null)
                {
                    WaitingMessages.Add(def);
                    ans = true;
                }
            }
            return ans;
        }

        /// <summary>
        ///     获取信息的检索关键字
        /// </summary>
        /// <param name="message">待确认的信息</param>
        /// <returns>信息的检索关键字</returns>
        protected (string, string)? GetKeyFromMessage(byte[] message)
        {
            return null;
        }

        /// <inheritdoc />
        public ICollection<(byte[], bool)> ConfirmMessage(byte[] receiveMessage)
        {
            var ans = new List<(byte[], bool)>
            {
                (receiveMessage, true)
            };
            return ans;
        }

        /// <summary>
        ///     从等待队列中匹配信息
        /// </summary>
        /// <param name="receiveMessage">返回的信息</param>
        /// <returns>从等待队列中匹配的信息</returns>
        protected MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage)
        {
            MessageWaitingDef ans;
            lock (WaitingMessages)
            {
                ans = WaitingMessages.FirstOrDefault();
            }
            return ans;
        }

        /// <inheritdoc />
        public void ForceRemoveWaitingMessage(MessageWaitingDef def)
        {
            lock (WaitingMessages)
            {
                if (WaitingMessages.IndexOf(def) >= 0)
                {
                    WaitingMessages.Remove(def);
                }
            }
        }
    }
}