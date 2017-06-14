namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Da协议Api入口
    /// </summary>
    public class OpcDaUtility : OpcUtility
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionString">连接地址</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcDaUtility(string connectionString, bool isRegexOn = false) : base(connectionString)
        {
            Wrapper = new OpcDaProtocal(ConnectionString, isRegexOn);
        }
    }
}