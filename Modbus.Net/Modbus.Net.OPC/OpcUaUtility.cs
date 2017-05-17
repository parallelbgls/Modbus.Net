namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Da协议Api入口
    /// </summary>
    public class OpcUaUtility : OpcUtility
    {
        public OpcUaUtility(string connectionString) : base(connectionString)
        {
            Wrapper = new OpcUaProtocal(ConnectionString);
        }
    }
}