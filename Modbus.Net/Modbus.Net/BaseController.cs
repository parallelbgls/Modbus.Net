using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Modbus.Net
{
    public abstract class BaseController : IController
    { 
        protected List<MessageWaitingDef> WaitingMessages { get; set; }

        protected Task SendingThread { get; set; }
        protected bool _taskCancel = false;

        protected BaseController()
        {
            WaitingMessages = new List<MessageWaitingDef>();
        }

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

        protected abstract void SendingMessageControlInner();

        public void SendStop()
        {
            _taskCancel = true;
        }

        public void SendStart()
        {
            if (SendingThread == null)
            {
                SendingThread = Task.Run(()=>SendingMessageControlInner());               
            }
        }

        public void Clear()
        {
            lock (WaitingMessages)
            {
                WaitingMessages.Clear();
            }
        }

        protected virtual void AddMessageToList(MessageWaitingDef def)
        {
            lock (WaitingMessages)
            {
                WaitingMessages.Add(def);
            }
        }

        protected abstract string GetKeyFromMessage(byte[] message);

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

        protected abstract MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage);

        public void ForceRemoveWaitingMessage(MessageWaitingDef def)
        {
            lock (WaitingMessages)
            {
                WaitingMessages.Remove(def);
            }
        }
    }

    public class MessageWaitingDef
    {
        public string Key { get; set; }

        public byte[] SendMessage { get; set; }

        public byte[] ReceiveMessage { get; set; }

        public EventWaitHandle SendMutex { get; set; }

        public EventWaitHandle ReceiveMutex { get; set; }
    }
}