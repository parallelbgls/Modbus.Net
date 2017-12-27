using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     匹配控制器，载入队列后直接发送
    /// </summary>
    public class MatchDirectlySendController : MatchController
    {
        /// <inheritdoc />
        public MatchDirectlySendController(ICollection<(int, int)>[] keyMatches, int acquireTime) : base(keyMatches,
            acquireTime)
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
