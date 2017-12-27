using System;
using System.Linq;
using System.Threading;
using Serilog;

namespace Modbus.Net
{
    /// <summary>
    ///     先入先出式控制器
    /// </summary>
    public class FifoController : BaseController
    {
        private MessageWaitingDef _currentSendingPos;

        private bool _taskCancel = false;

        private int _waitingListMaxCount = 10000;

        private readonly Semaphore _taskCycleSema;

        /// <summary>
        ///     间隔时间
        /// </summary>
        public int AcquireTime { get; }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="acquireTime">间隔时间</param>
        public FifoController(int acquireTime)
        {
            _taskCycleSema = new Semaphore(0, _waitingListMaxCount);
            AcquireTime = acquireTime;
        }

        /// <inheritdoc />
        protected override void SendingMessageControlInner()
        {
            try
            {
                _taskCycleSema.WaitOne();
                while (!_taskCancel)
                {
                    if (AcquireTime > 0)
                    {
                        Thread.Sleep(AcquireTime);
                    }
                    bool sendSuccess = false;
                    lock (WaitingMessages)
                    {
                        if (_currentSendingPos == null)
                        {
                            if (WaitingMessages.Count > 0)
                            {
                                _currentSendingPos = WaitingMessages.First();
                                _currentSendingPos.SendMutex.Set();
                                sendSuccess = true;
                            }
                        }
                        else 
                        {
                            if (WaitingMessages.Count <= 0)
                            {
                                _currentSendingPos = null;
                                _taskCycleSema.Close();
                                sendSuccess = true;
                            }
                            else if (WaitingMessages.Count > WaitingMessages.IndexOf(_currentSendingPos) + 1)
                            {
                                _currentSendingPos = WaitingMessages[WaitingMessages.IndexOf(_currentSendingPos) + 1];
                                _currentSendingPos.SendMutex.Set();
                                sendSuccess = true;
                            }
                        }
                    }
                    if (sendSuccess)
                    { 
                        _taskCycleSema.WaitOne();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                //ignore
            }
            catch (Exception e)
            {
                Log.Error(e, "Controller throws exception");
            }

        }

        /// <inheritdoc />
        public override void SendStart()
        {
            _taskCancel = false;
            base.SendStart();
        }

        /// <inheritdoc />
        public override void SendStop()
        {
            _taskCancel = true;
        }

        /// <inheritdoc />
        protected override (string,string)? GetKeyFromMessage(byte[] message)
        {
            return null;
        }

        /// <inheritdoc />
        protected override MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage)
        {
            return WaitingMessages.FirstOrDefault();
        }

        /// <inheritdoc />
        protected override bool AddMessageToList(MessageWaitingDef def)
        {
            if (WaitingMessages.Count > _waitingListMaxCount)
            {
                return false;
            }
            var success = base.AddMessageToList(def);
            if (success)
            {
                _taskCycleSema.Release();
            }
            return success;
        }
    }
}
