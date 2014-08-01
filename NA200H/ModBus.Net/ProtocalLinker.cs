using System.Reflection;

namespace ModBus.Net
{
    /// <summary>
    /// 基本的协议连接器
    /// </summary>
    public abstract class ProtocalLinker
    {
        /// <summary>
        /// 发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public abstract byte[] SendReceive(byte[] content);

        /// <summary>
        /// 仅发送数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>协议是否正确发送</returns>
        public abstract bool SendOnly(byte[] content);

        /// <summary>
        /// 检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public abstract bool CheckRight(byte[] content);

        /// <summary>
        /// 协议内容扩展，发送时根据需要扩展
        /// </summary>
        /// <param name="content">扩展前的基本协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public byte[] BytesExtend(byte[] content)
        {
            //自动查找相应的协议放缩类，命令规则为——当前的实际类名（注意是继承后的）+"BytesExtend"。
            ProtocalLinkerBytesExtend bytesExtend =
                Assembly.Load("ModBus.Net").CreateInstance(this.GetType().FullName + "BytesExtend") as
                    ProtocalLinkerBytesExtend;
            return bytesExtend.BytesExtend(content);
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
                Assembly.Load("ModBus.Net").CreateInstance(this.GetType().FullName + "BytesExtend") as
                    ProtocalLinkerBytesExtend;
            return bytesExtend.BytesDecact(content);
        }
    }
}