using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Modbus.Net
{
    public class FifoController : BaseController
    {
        private MessageWaitingDef _currentSendingPos;

        public int AcquireTime { get; }

        public FifoController(int acquireTime)
        {
            AcquireTime = acquireTime;
        }

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

        protected override string GetKeyFromMessage(byte[] message)
        {
            return null;
        }

        protected override MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage)
        {
            return WaitingMessages.FirstOrDefault();
        }
    }
}
