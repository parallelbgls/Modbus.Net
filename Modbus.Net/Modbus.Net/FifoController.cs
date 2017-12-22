using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            AcquireTime = acquireTime;
        }

        /// <inheritdoc cref="BaseController.SendingMessageControlInner"/>
        protected override void SendingMessageControlInner()
        {
            try
            {
                while (!_taskCancel)
                {
                    if (AcquireTime > 0)
                    {
                        Thread.Sleep(AcquireTime);
                    }
                    lock (WaitingMessages)
                    {
                        if (_currentSendingPos == null)
                        {
                            if (WaitingMessages.Count > 0)
                            {
                                _currentSendingPos = WaitingMessages.First();
                            }
                        }
                        if (_currentSendingPos != null)
                        {
                            _currentSendingPos.SendMutex.Set();
                            if (WaitingMessages.Count <= 1)
                            {
                                _currentSendingPos = null;
                            }
                            else
                            {
                                _currentSendingPos = WaitingMessages[WaitingMessages.IndexOf(_currentSendingPos) + 1];
                            }
                        }
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

        /// <inheritdoc cref="BaseController.SendStop"/>
        public override void SendStop()
        {
            _taskCancel = true;
        }

        /// <inheritdoc cref="BaseController.GetKeyFromMessage(byte[])"/>
        protected override string GetKeyFromMessage(byte[] message)
        {
            return null;
        }

        /// <inheritdoc cref="BaseController.GetMessageFromWaitingList(byte[])"/>
        protected override MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage)
        {
            return WaitingMessages.FirstOrDefault();
        }
    }
}
