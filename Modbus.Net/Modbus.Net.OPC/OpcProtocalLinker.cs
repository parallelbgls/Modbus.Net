using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc协议连接器
    /// </summary>
    public abstract class OpcProtocalLinker : ProtocalLinker<OpcParamIn, OpcParamOut>
    {
        public override bool? CheckRight(OpcParamOut content)
        {
            if (content == null || !content.Success) return false;
            if (content.Value.Length == 6 && Encoding.ASCII.GetString(content.Value) == "NoData")
            {
                return null;
            }
            return base.CheckRight(content);
        }
    }
}
