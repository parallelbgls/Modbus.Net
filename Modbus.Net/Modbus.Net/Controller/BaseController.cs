using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     ����������
    /// </summary>
    public abstract class BaseController : IController
    {
        /// <summary>
        ///     �ȴ�����Ϣ����
        /// </summary>
        protected List<MessageWaitingDef> WaitingMessages { get; set; }

        /// <summary>
        ///     ��Ϣά���߳�
        /// </summary>
        protected Task SendingThread { get; set; }

        /// <summary>
        ///     ��Ϣά���߳��Ƿ�������
        /// </summary>
        public virtual bool IsSending => SendingThread != null;

        private CancellationTokenSource _sendingThreadCancel;

        /// <summary>
        ///     ���з�λ�ú���
        /// </summary>
        protected Func<byte[], int> LengthCalc { get; }

        /// <summary>
        ///     ��У�麯��
        /// </summary>
        protected Func<byte[], bool?> CheckRightFunc { get; }

        /// <summary>
        ///     ������
        /// </summary>
        /// <param name="lengthCalc">�����ȼ��㺯��</param>
        /// <param name="checkRightFunc">��У�麯��</param>
        protected BaseController(Func<byte[], int> lengthCalc = null, Func<byte[], bool?> checkRightFunc = null)
        {
            WaitingMessages = new List<MessageWaitingDef>();
            LengthCalc = lengthCalc;
            CheckRightFunc = checkRightFunc;
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
        ///     ������Ϣ��ʵ���ڲ�����
        /// </summary>
        protected abstract void SendingMessageControlInner(CancellationToken token);

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
        ///     ����Ϣ��ӵ�����
        /// </summary>
        /// <param name="def">��Ҫ��ӵ���Ϣ��Ϣ</param>
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
        ///     ��ȡ��Ϣ�ļ����ؼ���
        /// </summary>
        /// <param name="message">��ȷ�ϵ���Ϣ</param>
        /// <returns>��Ϣ�ļ����ؼ���</returns>
        protected abstract (string, string)? GetKeyFromMessage(byte[] message);

        /// <inheritdoc />
        public ICollection<(byte[], bool)> ConfirmMessage(byte[] receiveMessage)
        {
            var ans = new List<(byte[], bool)>();
            byte[] receiveMessageCopy = new byte[receiveMessage.Length];
            Array.Copy(receiveMessage, receiveMessageCopy, receiveMessage.Length);
            int? length = -1;
            try
            {
                length = LengthCalc?.Invoke(receiveMessageCopy);
            }
            catch
            {
                //ignore
            }
            List<(byte[], bool)> duplicatedMessages;
            if (length == null || length == -1) return ans;
            else if (length == 0) return null;
            else
            {
                duplicatedMessages = new List<(byte[], bool)>();
                var skipLength = 0;
                while (receiveMessageCopy.Length >= length)
                {
                    var duplicateMessage = receiveMessageCopy.Take(length.Value).ToArray();
                    if (CheckRightFunc != null && CheckRightFunc(duplicateMessage) == false)
                    {
                        receiveMessageCopy = receiveMessageCopy.TakeLast(receiveMessageCopy.Length - 1).ToArray();
                        skipLength++;
                        continue;
                    }
                    if (skipLength > 0)
                    {
                        duplicatedMessages.Add((new byte[skipLength], false));
                    }
                    skipLength = 0;
                    duplicatedMessages.Add((duplicateMessage, true));
                    receiveMessageCopy = receiveMessageCopy.TakeLast(receiveMessageCopy.Length - length.Value).ToArray();
                    if (receiveMessageCopy.Length == 0) break;
                    length = LengthCalc?.Invoke(receiveMessageCopy);
                    if (length == -1) break;
                    if (length == 0) return null;
                }
                if (skipLength > 0)
                {
                    lock (WaitingMessages)
                    {
                        var def = GetMessageFromWaitingList(null);
                        if (def != null)
                        {
                            lock (WaitingMessages)
                            {
                                if (WaitingMessages.IndexOf(def) >= 0)
                                {
                                    WaitingMessages.Remove(def);
                                }
                            }
                            def.ReceiveMutex.Set();
                        }
                    }
                    return null;
                }
            }
            foreach (var message in duplicatedMessages)
            {
                if (!message.Item2)
                {
                    ans.Add((message.Item1, true));
                }
                else
                {
                    var def = GetMessageFromWaitingList(message.Item1);
                    if (def != null)
                    {
                        def.ReceiveMessage = message.Item1;
                        ForceRemoveWaitingMessage(def);
                        def.ReceiveMutex.Set();
                        ans.Add((message.Item1, true));
                    }
                    else
                    {
                        ans.Add((message.Item1, false));
                    }
                }
            }
            return ans;
        }

        /// <summary>
        ///     �ӵȴ�������ƥ����Ϣ
        /// </summary>
        /// <param name="receiveMessage">���ص���Ϣ</param>
        /// <returns>�ӵȴ�������ƥ�����Ϣ</returns>
        protected abstract MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage);

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

    /// <summary>
    ///     �ȴ���Ϣ�Ķ���
    /// </summary>
    public class MessageWaitingDef
    {
        /// <summary>
        ///     ��Ϣ�Ĺؼ���
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     ���͵���Ϣ
        /// </summary>
        public byte[] SendMessage { get; set; }

        /// <summary>
        ///     ���յ���Ϣ
        /// </summary>
        public byte[] ReceiveMessage { get; set; }

        /// <summary>
        ///     ���͵��ź�
        /// </summary>
        public EventWaitHandle SendMutex { get; set; }

        /// <summary>
        ///     ���յ��ź�
        /// </summary>
        public EventWaitHandle ReceiveMutex { get; set; }
    }
}