using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc协议连接器
    /// </summary>
    public abstract class OpcProtocolLinker : ProtocolLinker<OpcParamIn, OpcParamOut>
    {
        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public override async Task<OpcParamOut> SendReceiveAsync(OpcParamIn content)
        {
            var extBytes = BytesExtend(content);
            var receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
            return receiveBytes == null
                ? null
                : receiveBytes.Value.Length == 0 ? receiveBytes : BytesDecact(receiveBytes);
        }

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public override async Task<OpcParamOut> SendReceiveWithoutExtAndDecAsync(OpcParamIn content)
        {
            //发送数据
            var receiveBytes = await BaseConnector.SendMsgAsync(content);
            //容错处理
            var checkRight = CheckRight(receiveBytes);
            return checkRight == null
                ? new OpcParamOut {Success = false, Value = new byte[0]}
                : (!checkRight.Value ? null : receiveBytes);
            //返回字符
        }

        /// <summary>
        ///     检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public override bool? CheckRight(OpcParamOut content)
        {
            if (content == null || !content.Success) return false;
            if (content.Value.Length == 6 && Encoding.ASCII.GetString(content.Value) == "NoData")
                return null;
            return base.CheckRight(content);
        }
    }
}