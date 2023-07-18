using System;
using System.Collections.Generic;
using System.Threading;

namespace Modbus.Net
{
    /// <summary>
    ///     匹配控制器，载入队列后直接发送
    /// </summary>
    public class MatchDirectlySendController : MatchController
    {
        /// <summary>
        ///     消息维护线程是否在运行
        /// </summary>
        public override bool IsSending => true;

        /// <inheritdoc />
        public MatchDirectlySendController(ICollection<(int, int)>[] keyMatches,
            Func<byte[], int> lengthCalc = null, Func<byte[], bool?> checkRightFunc = null, int? waitingListMaxCount = null) : base(keyMatches,
            0, lengthCalc, checkRightFunc, waitingListMaxCount)
        {
        }

        /// <inheritdoc />
        protected override bool AddMessageToList(MessageWaitingDef def)
        {
            var ans = base.AddMessageToList(def);
            if (ans)
            {
                def.SendMutex.Set();
            }
            return ans;
        }

        /// <inheritdoc />
        protected override void SendingMessageControlInner(CancellationToken token)
        {
            //empty
        }
    }
}
