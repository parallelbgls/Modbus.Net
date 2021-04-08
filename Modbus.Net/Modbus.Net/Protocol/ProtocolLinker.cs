using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     基本的协议连接器
    /// </summary>
    public abstract class ProtocolLinker : ProtocolLinker<byte[], byte[]>
    {
        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public override async Task<byte[]> SendReceiveAsync(byte[] content)
        {
            var extBytes = BytesExtend(content);
            var receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
            return receiveBytes == null ? null : receiveBytes.Length == 0 ? receiveBytes : BytesDecact(receiveBytes);
        }

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public override async Task<byte[]> SendReceiveWithoutExtAndDecAsync(byte[] content)
        {
            //发送数据
            var receiveBytes = await BaseConnector.SendMsgAsync(content);
            //容错处理
            var checkRight = CheckRight(receiveBytes);
            return checkRight == null ? new byte[0] : (!checkRight.Value ? null : receiveBytes);
            //返回字符
        }

        /// <summary>
        ///     协议内容扩展，发送时根据需要扩展
        /// </summary>
        /// <param name="content">扩展前的基本协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public override byte[] BytesExtend(byte[] content)
        {
            //自动查找相应的协议放缩类，命令规则为——当前的实际类名（注意是继承后的）+"BytesExtend"。
            var bytesExtend =
                Activator.CreateInstance(GetType().GetTypeInfo().Assembly.GetType(GetType().FullName + "BytesExtend"))
                    as
                    IProtocolLinkerBytesExtend;
            return bytesExtend?.BytesExtend(content);
        }

        /// <summary>
        ///     协议内容缩减，接收时根据需要缩减
        /// </summary>
        /// <param name="content">缩减前的完整协议内容</param>
        /// <returns>缩减后的协议内容</returns>
        public override byte[] BytesDecact(byte[] content)
        {
            //自动查找相应的协议放缩类，命令规则为——当前的实际类名（注意是继承后的）+"BytesExtend"。
            var bytesExtend =
                Activator.CreateInstance(GetType().GetTypeInfo().Assembly.GetType(GetType().FullName + "BytesExtend"))
                    as
                    IProtocolLinkerBytesExtend;
            return bytesExtend?.BytesDecact(content);
        }

        /// <summary>
        ///     检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public override bool? CheckRight(byte[] content)
        {
            if (content == null)
            {
                Disconnect();
                return false;
            }
            if (content.Length == 0) return null;
            return true;
        }
    }
    
    /// <summary>
    ///     基本的协议连接器
    /// </summary>
    public abstract class ProtocolLinker<TParamIn, TParamOut> : IProtocolLinker<TParamIn, TParamOut>
        where TParamOut : class
    {
        /// <summary>
        ///     传输连接器
        /// </summary>
        protected IConnector<TParamIn, TParamOut> BaseConnector;

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            return await BaseConnector.ConnectAsync();
        }

        /// <summary>
        ///     断开设备
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        public bool Disconnect()
        {
            return BaseConnector.Disconnect();
        }

        /// <summary>
        ///     通讯字符串
        /// </summary>
        public string ConnectionToken => BaseConnector.ConnectionToken;

        /// <summary>
        ///     设备是否连接
        /// </summary>
        public bool IsConnected => BaseConnector != null && BaseConnector.IsConnected;

        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual TParamOut SendReceive(TParamIn content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(content));
        }

        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual async Task<TParamOut> SendReceiveAsync(TParamIn content)
        {
            var extBytes = BytesExtend(content);
            var receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
            return BytesDecact(receiveBytes);
        }

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual TParamOut SendReceiveWithoutExtAndDec(TParamIn content)
        {
            return AsyncHelper.RunSync(() => SendReceiveWithoutExtAndDecAsync(content));
        }

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual async Task<TParamOut> SendReceiveWithoutExtAndDecAsync(TParamIn content)
        {
            //发送数据
            var receiveBytes = await BaseConnector.SendMsgAsync(content);
            //容错处理
            var checkRight = CheckRight(receiveBytes);
            //返回字符
            return checkRight == true ? receiveBytes : null;
        }

        /// <summary>
        ///     检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public virtual bool? CheckRight(TParamOut content)
        {
            if (content != null) return true;
            Disconnect();
            return false;
        }

        /// <summary>
        ///     协议内容扩展，发送时根据需要扩展
        /// </summary>
        /// <param name="content">扩展前的基本协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public virtual TParamIn BytesExtend(TParamIn content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     协议内容缩减，接收时根据需要缩减
        /// </summary>
        /// <param name="content">缩减前的完整协议内容</param>
        /// <returns>缩减后的协议内容</returns>
        public virtual TParamOut BytesDecact(TParamOut content)
        {
            throw new NotImplementedException();
        }
    }
}