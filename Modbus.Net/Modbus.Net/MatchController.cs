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
    ///     通讯号匹配模式的控制器
    /// </summary>
    public class MatchController : BaseController
    {
        private MessageWaitingDef _currentSendingPos;

        private bool _taskCancel = false;

        /// <summary>
        ///     获取间隔
        /// </summary>
        public int AcquireTime { get; }

        /// <summary>
        ///     匹配字典
        /// </summary>
        protected ICollection<int>[] KeyMatches { get; }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="keyMatches">匹配字典，每个Collection代表一个匹配集合，每一个匹配集合中的数字代表需要匹配的位置，最后计算出来的数字是所有位置数字按照集合排序后叠放在一起</param>
        /// <param name="acquireTime">获取间隔</param>
        public MatchController(ICollection<int>[] keyMatches, int acquireTime)
        {
            KeyMatches = keyMatches;
            AcquireTime = acquireTime;
        }

        /// <inheritdoc />
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
                            _currentSendingPos = WaitingMessages.Count <= 1
                                ? null
                                : WaitingMessages[WaitingMessages.IndexOf(_currentSendingPos) + 1];
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

        /// <inheritdoc />
        public override void SendStop()
        {
            _taskCancel = false;
        }

        /// <inheritdoc />
        protected override string GetKeyFromMessage(byte[] message)
        {
            string ans = "";
            foreach (var matchPoses in KeyMatches)
            {
                int tmpCount = 0;
                foreach (var matchPos in matchPoses)
                {
                    tmpCount = tmpCount * 256 + message[matchPos];
                }
                ans += tmpCount + " ";
            }
            return ans;
        }

        /// <inheritdoc />
        protected override MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage)
        {
            var returnKey = GetKeyFromMessage(receiveMessage);
            return WaitingMessages.FirstOrDefault(p=>p.Key == returnKey);
        }
    }
}
