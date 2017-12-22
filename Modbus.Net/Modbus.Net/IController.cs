namespace Modbus.Net
{
    /// <summary>
    ///     传输控制器接口
    /// </summary>
    public interface IController
    {
        /// <summary>
        ///     增加信息
        /// </summary>
        /// <param name="sendMessage">需要发送的信息</param>
        /// <returns></returns>
        MessageWaitingDef AddMessage(byte[] sendMessage);

        /// <summary>
        ///     启动传输控制线程
        /// </summary>
        void SendStart();

        /// <summary>
        ///     关闭传输控制线程
        /// </summary>
        void SendStop();

        /// <summary>
        ///     清空所有待发送的信息
        /// </summary>
        void Clear();

        /// <summary>
        ///     将返回的信息绑定到发送的信息上，并对信息进行确认
        /// </summary>
        /// <param name="receiveMessage">返回的信息</param>
        /// <returns>是否正常确认</returns>
        bool ConfirmMessage(byte[] receiveMessage);

        /// <summary>
        ///     没有任何返回时强行删除等待队列上的信息
        /// </summary>
        /// <param name="def">需要强行删除的信息</param>
        void ForceRemoveWaitingMessage(MessageWaitingDef def);
    }
}
