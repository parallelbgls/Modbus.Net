namespace Modbus.Net
{
    /// <summary>
    ///     基础的协议连接接口
    /// </summary>
    public interface IConnectorWithController<in TParamIn, TParamOut> : IConnector<TParamIn, TParamOut>
    {
        /// <summary>
        ///     增加传输控制器
        /// </summary>
        /// <param name="controller">传输控制器</param>
        void AddController(IController controller);
    }
}