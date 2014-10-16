using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public abstract class BaseConnector
    {
        public abstract bool IsConnected { get; }
        /// <summary>
        /// 连接PLC
        /// </summary>
        /// <returns></returns>
        public abstract bool Connect();
        /// <summary>
        /// 断开PLC
        /// </summary>
        /// <returns></returns>
        public abstract bool Disconnect();
        /// <summary>
        /// 无返回发送数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool SendMsgWithoutReturn(byte[] message);
        /// <summary>
        /// 带返回发送数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract byte[] SendMsg(byte[] message);
    }
}
