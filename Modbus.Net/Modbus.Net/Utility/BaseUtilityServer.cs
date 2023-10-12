using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     基础Api入口
    /// </summary>
    public abstract class BaseUtilityServer<TParamIn, TParamOut, TProtocolUnit, TPipeUnit> : IUtilityServer
        where TProtocolUnit : class, IProtocolFormatting<TParamIn, TParamOut> where TParamOut : class
        where TPipeUnit : PipeUnit<TParamIn, TParamOut, IProtocolLinker<TParamIn, TParamOut>, TProtocolUnit>
    {
        private static readonly ILogger<BaseUtilityServer<TParamIn, TParamOut, TProtocolUnit, TPipeUnit>> logger = LogProvider.CreateLogger<BaseUtilityServer<TParamIn, TParamOut, TProtocolUnit, TPipeUnit>>();

        /// <summary>
        ///     协议收发主体
        /// </summary>
        protected IProtocol<TParamIn, TParamOut, TProtocolUnit, TPipeUnit> Wrapper;

        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseUtilityServer(byte slaveAddress, byte masterAddress)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
            AddressTranslator = new AddressTranslatorBase();
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        protected string ConnectionString { get; set; }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; set; }

        /// <summary>
        ///     主站号
        /// </summary>
        public byte MasterAddress { get; set; }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的byte数据</returns>
        public abstract Task<ReturnStruct<byte[]>> GetServerDatasAsync(string startAddress, int getByteCount);

        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        public abstract Task<ReturnStruct<bool>> SetServerDatasAsync(string startAddress, object[] setContents);

        /// <summary>
        ///     协议是否遵循小端格式
        /// </summary>
        public abstract Endian Endian { get; }

        /// <summary>
        ///     设备是否已经连接
        /// </summary>
        public bool IsConnected => Wrapper?.ProtocolLinker != null && Wrapper.ProtocolLinker.IsConnected;

        /// <summary>
        ///     标识设备的连接关键字
        /// </summary>
        public string ConnectionToken
            => Wrapper?.ProtocolLinker == null ? ConnectionString : Wrapper.ProtocolLinker.ConnectionToken;

        /// <summary>
        ///     地址翻译器
        /// </summary>
        public AddressTranslator AddressTranslator { get; set; }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            return await Wrapper.ConnectAsync();
        }

        /// <summary>
        ///     断开设备
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        public bool Disconnect()
        {
            return Wrapper.Disconnect();
        }

        /// <summary>
        ///     返回Utility的方法集合
        /// </summary>
        /// <typeparam name="TUtilityMethod">Utility方法集合类型</typeparam>
        /// <returns>Utility方法集合</returns>
        public TUtilityMethod GetUtilityMethods<TUtilityMethod>() where TUtilityMethod : class, IUtilityMethod
        {
            if (this is TUtilityMethod)
            {
                return this as TUtilityMethod;
            }
            return null;
        }

        /// <summary>
        ///     设置连接类型
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        public abstract void SetConnectionType(int connectionType);
    }
}