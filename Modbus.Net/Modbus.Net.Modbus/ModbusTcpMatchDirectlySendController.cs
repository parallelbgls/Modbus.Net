using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net
{
    /// <summary>
    ///     匹配控制器，载入队列后直接发送
    /// </summary>
    public class ModbusTcpMatchDirectlySendController : MatchDirectlySendController
    {
        /// <inheritdoc />
        public ModbusTcpMatchDirectlySendController(ICollection<(int, int)>[] keyMatches,
            Func<byte[], int> lengthCalc = null, int? waitingListMaxCount = null) : base(keyMatches,
            lengthCalc, waitingListMaxCount)
        {
        }

        /// <inheritdoc />
        protected override MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage)
        {
            MessageWaitingDef ans;
            if (receiveMessage[0] == 0 && receiveMessage[1] == 0)
            {
                lock (WaitingMessages)
                {
                    ans = WaitingMessages.FirstOrDefault();
                }
            }
            else
            {
                var returnKey = GetKeyFromMessage(receiveMessage);
                lock (WaitingMessages)
                {
                    ans = WaitingMessages.FirstOrDefault(p => returnKey.HasValue && p.Key == returnKey.Value.Item2);
                }
            }
            return ans;
        }
    }
}
