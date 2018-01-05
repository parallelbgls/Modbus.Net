using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net
{   
    public class MessageReturnArgs<TParamOut>
    {
        public TParamOut ReturnMessage { get; set; }
    }



    public class MessageReturnCallbackArgs<TParamIn>
    {
        public TParamIn SendMessage { get; set; }
    }
}
