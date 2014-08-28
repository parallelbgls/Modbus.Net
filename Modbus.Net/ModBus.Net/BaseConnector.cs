using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public abstract class BaseConnector
    {
        public abstract bool Connect();
        public abstract bool Disconnect();
        public abstract bool SendMsgWithoutReturn(byte[] message);
        public abstract byte[] SendMsg(byte[] message);
    }
}
