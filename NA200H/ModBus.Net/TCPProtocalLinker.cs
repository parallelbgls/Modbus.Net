namespace ModBus.Net
{
    public class TCPProtocalLinker : ProtocalLinker
    {
        private static TCPSocket _socket;

        public TCPProtocalLinker()
        {
            if (_socket == null)
            {
                _socket = new TCPSocket(ConfigurationManager.IP, int.Parse(ConfigurationManager.Port), false);
            }
        }

        public override byte[] SendReceive(byte[] content)
        {
            return _socket.SendMsg(content);
        }

        public override bool SendOnly(byte[] content)
        {
            return _socket.SendMsgWithoutReturn(content);
        }
    }
}