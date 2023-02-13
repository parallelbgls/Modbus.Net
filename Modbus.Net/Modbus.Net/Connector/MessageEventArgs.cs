namespace Modbus.Net
{   
    /// <summary>
    ///     数据返回代理参数
    /// </summary>
    /// <typeparam name="TParamOut"></typeparam>
    public class MessageReturnArgs<TParamOut>
    {
        /// <summary>
        ///     返回的数据
        /// </summary>
        public TParamOut ReturnMessage { get; set; }
    }

    
    /// <summary>
    ///     数据发送代理参数
    /// </summary>
    /// <typeparam name="TParamIn"></typeparam>
    public class MessageReturnCallbackArgs<TParamIn>
    {
        /// <summary>
        ///     发送的数据
        /// </summary>
        public TParamIn SendMessage { get; set; }
    }
}
