using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Modbus.Net
{
    /// <summary>
    ///     控制器基类
    /// </summary>
    public abstract class BaseController : IController
    { 
        /// <summary>
        ///     等待的消息队列
        /// </summary>
        protected List<MessageWaitingDef> WaitingMessages { get; set; }

        /// <summary>
        ///     消息维护线程
        /// </summary>
        protected Task SendingThread { get; set; }

        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseController()
        {
            WaitingMessages = new List<MessageWaitingDef>();
        }

        /// <inheritdoc cref="IController.AddMessage(byte[])" />
        public MessageWaitingDef AddMessage(byte[] sendMessage)
        {
            var def = new MessageWaitingDef
            {
                Key = GetKeyFromMessage(sendMessage),
                SendMessage = sendMessage,
                SendMutex = new AutoResetEvent(false),
                ReceiveMutex = new AutoResetEvent(false)
            };
            AddMessageToList(def);         
            return def;
        }

        /// <summary>
        ///     发送消息的实际内部方法
        /// </summary>
        protected abstract void SendingMessageControlInner();

        /// <inheritdoc cref="IController.SendStop" />
        public abstract void SendStop();

        /// <inheritdoc cref="IController.SendStart" />
        public void SendStart()
        {
            if (SendingThread == null)
            {
                SendingThread = Task.Run(()=>SendingMessageControlInner());               
            }
        }

        /// <inheritdoc cref="IController.Clear" />
        public void Clear()
        {
            lock (WaitingMessages)
            {
                WaitingMessages.Clear();
            }
        }

        /// <summary>
        ///     将信息添加到队列
        /// </summary>
        /// <param name="def">需要添加的信息信息</param>
        protected virtual void AddMessageToList(MessageWaitingDef def)
        {
            lock (WaitingMessages)
            {
                WaitingMessages.Add(def);
            }
        }

        /// <summary>
        ///     获取信息的检索关键字
        /// </summary>
        /// <param name="message">待确认的信息</param>
        /// <returns>信息的检索关键字</returns>
        protected abstract string GetKeyFromMessage(byte[] message);

        /// <inheritdoc cref="IController.ConfirmMessage(byte[])" />
        public bool ConfirmMessage(byte[] receiveMessage)
        {
            var def = GetMessageFromWaitingList(receiveMessage);
            if (def != null)
            {
                def.ReceiveMessage = receiveMessage;
                lock (WaitingMessages)
                {
                    WaitingMessages.Remove(def);
                }
                def.ReceiveMutex.Set();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     从等待队列中匹配信息
        /// </summary>
        /// <param name="receiveMessage">返回的信息</param>
        /// <returns>从等待队列中匹配的信息</returns>
        protected abstract MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage);

        /// <inheritdoc cref="IController.ForceRemoveWaitingMessage(MessageWaitingDef)"/>
        /// <param name="def"></param>
        public void ForceRemoveWaitingMessage(MessageWaitingDef def)
        {
            lock (WaitingMessages)
            {
                WaitingMessages.Remove(def);
            }
        }
    }

    /// <summary>
    ///     等待信息的定义
    /// </summary>
    public class MessageWaitingDef
    {
        /// <summary>
        ///     信息的关键字
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     发送的信息
        /// </summary>
        public byte[] SendMessage { get; set; }

        /// <summary>
        ///     接收的信息
        /// </summary>
        public byte[] ReceiveMessage { get; set; }

        /// <summary>
        ///     发送的信号
        /// </summary>
        public EventWaitHandle SendMutex { get; set; }

        /// <summary>
        ///     接收的信号
        /// </summary>
        public EventWaitHandle ReceiveMutex { get; set; }
    }
}