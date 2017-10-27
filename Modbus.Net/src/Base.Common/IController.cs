namespace Modbus.Net
{
    public interface IController
    {
        MessageWaitingDef AddMessage(byte[] sendMessage);

        void SendStop();

        void SendStart();

        void Clear();

        bool ConfirmMessage(byte[] receiveMessage);

        void ForceRemoveWaitingMessage(MessageWaitingDef def);
    }
}
