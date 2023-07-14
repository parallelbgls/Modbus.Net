using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;

namespace Modbus.Net
{
    /// <summary>
    ///     先入先出式控制器
    /// </summary>
    public class FifoController : BaseController
    {
        private static readonly ILogger<FifoController> logger = LogProvider.CreateLogger<FifoController>();

        private MessageWaitingDef _currentSendingPos;

        private bool _taskCancel = false;

        private bool _activateSema = false;

        private int _waitingListMaxCount;

        private Semaphore _taskCycleSema;

        /// <summary>
        ///     间隔时间
        /// </summary>
        public int AcquireTime { get; }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="acquireTime">间隔时间</param>
        /// <param name="activateSema">是否开启信号量</param>
        /// <param name="lengthCalc">包切分长度函数</param>
        /// <param name="checkRightFunc">包校验函数</param>
        /// <param name="waitingListMaxCount">包等待队列长度</param>
        public FifoController(int acquireTime, bool activateSema = true, Func<byte[], int> lengthCalc = null, Func<byte[], bool?> checkRightFunc = null, int? waitingListMaxCount = null)
            : base(lengthCalc, checkRightFunc)
        {
            _waitingListMaxCount = int.Parse(waitingListMaxCount != null ? waitingListMaxCount.ToString() : null ?? ConfigurationReader.GetValueDirect("Controller", "WaitingListCount"));
            _activateSema = activateSema;
            if (_activateSema)
            {
                _taskCycleSema = new Semaphore(0, _waitingListMaxCount);
            }
            AcquireTime = acquireTime;
        }

        /// <inheritdoc />
        protected override void SendingMessageControlInner()
        {
            _taskCycleSema?.WaitOne();
            while (!_taskCancel)
            {
                if (AcquireTime > 0)
                {
                    Thread.Sleep(AcquireTime);
                }
                bool sendSuccess = false;
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
                                sendSuccess = true;
                            }
                        }
                        else
                        {
                            if (WaitingMessages.Count <= 0)
                            {
                                _currentSendingPos = null;
                                _taskCycleSema?.Close();
                                sendSuccess = true;
                            }
                            else if (WaitingMessages.IndexOf(_currentSendingPos) == -1)
                            {
                                _currentSendingPos = WaitingMessages.First();
                                _currentSendingPos.SendMutex.Set();
                                sendSuccess = true;
                            }
                        }
                    }
                    catch (ObjectDisposedException e)
                    {
                        logger.LogError(e, "Controller _currentSendingPos disposed");
                        _currentSendingPos = null;
                        sendSuccess = true;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Controller throws exception");
                        _taskCancel = true;
                    }
                }
                if (sendSuccess)
                {
                    _taskCycleSema?.WaitOne();
                }
            }
            _taskCycleSema.Dispose();
            _taskCycleSema = null;
            Clear();
        }

        /// <inheritdoc />
        public override void SendStart()
        {
            if (_taskCycleSema == null && _activateSema)
            {
                _taskCycleSema = new Semaphore(0, _waitingListMaxCount);
            }
            _taskCancel = false;
            base.SendStart();
        }

        /// <inheritdoc />
        public override void SendStop()
        {
            _taskCancel = true;
        }

        /// <inheritdoc />
        protected override (string, string)? GetKeyFromMessage(byte[] message)
        {
            return null;
        }

        /// <inheritdoc />
        protected override MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage)
        {
            MessageWaitingDef ans;
            lock (WaitingMessages)
            {
                ans = WaitingMessages.FirstOrDefault();
            }
            return ans;
        }

        /// <inheritdoc />
        protected override bool AddMessageToList(MessageWaitingDef def)
        {
            if (WaitingMessages.Count > _waitingListMaxCount)
            {
                return false;
            }
            if (_taskCancel)
            {
                return false;
            }
            var success = base.AddMessageToList(def);
            if (success)
            {
                _taskCycleSema?.Release();
            }
            return success;
        }
    }
}
