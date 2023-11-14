using System;
using System.IO.Ports;
using System.Reflection;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     基本的协议连接器
    /// </summary>
    public abstract class ProtocolReceiver : ProtocolReceiver<byte[], byte[]>
    {
        protected ProtocolReceiver(string com, int slaveAddress, BaudRate? baudRate = null, Parity? parity = null, StopBits? stopBits = null, DataBits? dataBits = null, Handshake? handshake = null,
            int? connectionTimeout = null, bool? isFullDuplex = null)
        {
            baudRate = Enum.Parse<BaudRate>(baudRate != null ? baudRate.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "BaudRate"));
            parity = Enum.Parse<Parity>(parity != null ? parity.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "Parity"));
            stopBits = Enum.Parse<StopBits>(stopBits != null ? stopBits.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "StopBits"));
            dataBits = Enum.Parse<DataBits>(dataBits != null ? dataBits.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "DataBits"));
            handshake = Enum.Parse<Handshake>(handshake != null ? handshake.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "Handshake"));
            connectionTimeout = int.Parse(connectionTimeout != null ? connectionTimeout.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "ConnectionTimeout"));
            isFullDuplex = bool.Parse(isFullDuplex != null ? isFullDuplex.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "FullDuplex"));
            BaseConnector = new ComConnector(com + ":" + slaveAddress, baudRate.Value, parity.Value, stopBits.Value, dataBits.Value, handshake.Value, connectionTimeout.Value, isFullDuplex.Value);
            var noResponse = bool.Parse(ConfigurationReader.GetValue("COM:" + com, "NoResponse") ?? ConfigurationReader.GetValue("Controller", "NoResponse"));
            if (noResponse)
            {
                ((IConnectorWithController<byte[], byte[]>)BaseConnector).AddController(new NoResponseController(int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "FetchSleepTime"))));
            }
            else
            {
                this.AddController(new object[2] { com, slaveAddress }, BaseConnector);
            }
            BaseConnector.MessageReturn = receiveMessage => new MessageReturnCallbackArgs<byte[]>() { SendMessage = ReceiveSend(receiveMessage.ReturnMessage) };
        }

        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public override byte[] ReceiveSend(byte[] content)
        {
            var checkRight = CheckRight(content);
            if (checkRight == true)
            {
                var decBytes = BytesDecact(content);
                var explainContent = DataExplain(decBytes);
                var returnBytes = DataProcess(explainContent);
                if (returnBytes != null)
                {
                    var extBytes = BytesExtend(returnBytes);
                    return extBytes;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///     协议内容扩展，发送时根据需要扩展
        /// </summary>
        /// <param name="content">扩展前的基本协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public virtual byte[] BytesExtend(byte[] content)
        {
            //自动查找相应的协议放缩类，命令规则为——当前的实际类名（注意是继承后的）+"BytesExtend"。
            var bytesExtend =
                Activator.CreateInstance(GetType().GetTypeInfo().Assembly.GetType(GetType().FullName + "BytesExtend"))
                    as
                    IProtocolLinkerBytesExtend<byte[], byte[]>;
            return bytesExtend?.BytesExtend(content);
        }

        /// <summary>
        ///     协议内容缩减，接收时根据需要缩减
        /// </summary>
        /// <param name="content">缩减前的完整协议内容</param>
        /// <returns>缩减后的协议内容</returns>
        public virtual byte[] BytesDecact(byte[] content)
        {
            //自动查找相应的协议放缩类，命令规则为——当前的实际类名（注意是继承后的）+"BytesExtend"。
            var bytesExtend =
                Activator.CreateInstance(GetType().GetTypeInfo().Assembly.GetType(GetType().FullName + "BytesExtend"))
                    as
                    IProtocolLinkerBytesExtend<byte[], byte[]>;
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

        protected abstract Func<byte[], ReceiveDataDef> DataExplain { get; }

        public Func<ReceiveDataDef, byte[]> DataProcess { get; set; } = null;
    }

    public abstract class ProtocolReceiver<TParamIn, TParamOut> : IProtocolReceiver<TParamIn, TParamOut>
        where TParamIn : class
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

        public virtual Func<TParamOut, TParamIn> DispatchEvent
        {
            get
            {
                return receiveContent => ReceiveSend(receiveContent);
            }
        }

        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual TParamIn ReceiveSend(TParamOut content)
        {
            return ReceiveSendWithoutExtAndDec(content);
        }

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        public virtual TParamIn ReceiveSendWithoutExtAndDec(TParamOut content)
        {
            var checkRight = CheckRight(content);
            if (checkRight == true)
            {
                if (DispatchEvent != null)
                {
                    var returnContent = DispatchEvent(content);
                    return returnContent;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
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
    }

    public class ReceiveDataDef
    {
        public byte SlaveAddress { get; set; }

        public byte FunctionCode { get; set; }

        public ushort StartAddress { get; set; }

        public ushort Count { get; set; }

        public byte WriteByteCount { get; set; }

        public byte[] WriteContent { get; set; }
    }
}
