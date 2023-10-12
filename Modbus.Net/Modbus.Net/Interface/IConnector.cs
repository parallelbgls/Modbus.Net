using System;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     ������Э�����ӽӿ�
    /// </summary>
    public interface IConnector<TParamIn, TParamOut>
    {
        /// <summary>
        ///     ���ݷ��ش���
        /// </summary>
        Func<MessageReturnArgs<TParamOut>, MessageReturnCallbackArgs<TParamIn>> MessageReturn { get; set; }

        /// <summary>
        ///     ��ʶConnector�����ӹؼ���
        /// </summary>
        string ConnectionToken { get; }

        /// <summary>
        ///     �Ƿ�������״̬
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     ����PLC���첽
        /// </summary>
        /// <returns>�Ƿ����ӳɹ�</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        ///     �Ͽ�PLC
        /// </summary>
        /// <returns>�Ƿ�Ͽ��ɹ�</returns>
        bool Disconnect();

        /// <summary>
        ///     �����ط�������
        /// </summary>
        /// <param name="message">��Ҫ���͵�����</param>
        /// <returns>�Ƿ��ͳɹ�</returns>
        Task<TParamOut> SendMsgAsync(TParamIn message);
    }
}