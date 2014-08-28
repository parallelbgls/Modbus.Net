using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public abstract class RtuProtocalLinker : ProtocalLinker
    {
        protected RtuProtocalLinker()
        {
            //初始化连对象
            _baseConnector = new ComConnector(ConfigurationManager.COM);
        }

        protected RtuProtocalLinker(string com)
        {
            _baseConnector = new ComConnector(com);
        }
    }
}
