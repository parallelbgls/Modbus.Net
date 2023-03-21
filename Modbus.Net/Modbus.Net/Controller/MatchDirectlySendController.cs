using System;
using System.Collections.Generic;

namespace Modbus.Net
{
    /// <summary>
    ///     匹配控制器，载入队列后直接发送
    /// </summary>
    public class MatchDirectlySendController : MatchController
    {
        /// <inheritdoc />
        public MatchDirectlySendController(ICollection<(int, int)>[] keyMatches,
            Func<byte[], ICollection<byte[]>> duplicateFunc = null, int? waitingListMaxCount = null) : base(keyMatches,
            0, false, duplicateFunc, waitingListMaxCount)
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
        protected override void SendingMessageControlInner()
        {
            //empty
        }
    }
}
