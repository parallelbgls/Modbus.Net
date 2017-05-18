namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Ua协议Api入口
    /// </summary>
    public class OpcUaUtility : OpcUtility
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionString">连接地址</param>
        public OpcUaUtility(string connectionString) : base(connectionString)
        {
            Wrapper = new OpcUaProtocal(ConnectionString);
        }
    }
}