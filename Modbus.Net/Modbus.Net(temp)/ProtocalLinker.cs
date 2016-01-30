using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    /// 基本的协议连接器
    /// </summary>
    public abstract class ProtocalLinker
    {
        protected BaseConnector _baseConnector;

        public string ConnectionToken => _baseConnector.ConnectionToken;

        /// <summary>
        /// 设备是否连接
        /// </summary>
        public bool IsConnected => _baseConnector != null && _baseConnector.IsConnected;

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public bool Connect()
        {
            return _baseConnector.Connect();
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            return await _baseConnector.ConnectAsync();
        }
        
        /// <summary>
        /// 断开设备
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        public bool Disconnect()
        {
            return _baseConnector.Disconnect();
        }

        /// <summary>
        /// 发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual byte[] SendReceive(byte[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(content));
        }

        /// <summary>
        /// 发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual async Task<byte[]> SendReceiveAsync(byte[] content)
        {
            byte[] extBytes = BytesExtend(content);
            byte[] receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
            return receiveBytes == null ? null : BytesDecact(receiveBytes);
        }

        /// <summary>
        /// 发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual byte[] SendReceiveWithoutExtAndDec(byte[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveWithoutExtAndDecAsync(content));
        }

        /// <summary>
        /// 发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual async Task<byte[]> SendReceiveWithoutExtAndDecAsync(byte[] content)
        {
            //发送数据
            byte[] receiveBytes = await _baseConnector.SendMsgAsync(content);
            //容错处理
            return !CheckRight(receiveBytes) ? null : receiveBytes;
            //返回字符
        }

        /// <summary>
        /// 检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public virtual bool CheckRight(byte[] content)
        {
            if (content != null) return true;
            Disconnect();
            return false;
        }

        /// <summary>
        /// 协议内容扩展，发送时根据需要扩展
        /// </summary>
        /// <param name="content">扩展前的基本协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public byte[] BytesExtend(byte[] content)
        {
            //自动查找相应的协议放缩类，命令规则为——当前的实际类名（注意是继承后的）+"BytesExtend"。
            ProtocalLinkerBytesExtend bytesExtend =
                Assembly.Load(this.GetType().Assembly.FullName).CreateInstance(this.GetType().FullName + "BytesExtend") as
                    ProtocalLinkerBytesExtend;
            return bytesExtend?.BytesExtend(content);
        }

        /// <summary>
        /// 协议内容缩减，接收时根据需要缩减
        /// </summary>
        /// <param name="content">缩减前的完整协议内容</param>
        /// <returns>缩减后的协议内容</returns>
        public byte[] BytesDecact(byte[] content)
        {
            //自动查找相应的协议放缩类，命令规则为——当前的实际类名（注意是继承后的）+"BytesExtend"。
            ProtocalLinkerBytesExtend bytesExtend =
                Assembly.Load(this.GetType().Assembly.GetName().Name).CreateInstance(this.GetType().FullName + "BytesExtend") as
                    ProtocalLinkerBytesExtend;
            return bytesExtend?.BytesDecact(content);
        }
    }
}