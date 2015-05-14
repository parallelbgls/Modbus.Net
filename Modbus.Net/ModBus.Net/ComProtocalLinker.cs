using System;

namespace ModBus.Net
{
    public abstract class ComProtocalLinker : ProtocalLinker
    {
        protected ComProtocalLinker() : this(ConfigurationManager.COM)
        {

        }

        protected ComProtocalLinker(string com)
        {
            //初始化连对象
            _baseConnector = new ComConnector(com, 30000);
        }
    }
}
