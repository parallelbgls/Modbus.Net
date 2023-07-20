namespace Modbus.Net.HJ212
{
    public class HJ212Controller : FifoController
    {
        public HJ212Controller(string ip, int port) : base(int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "FetchSleepTime")))
        {

        }
    }
}
