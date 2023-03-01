using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     Machine��д�����ӿ�
    /// </summary>
    public interface IMachineMethod
    {
    }

    /// <summary>
    ///     Machine�����ݶ�д�ӿ�
    /// </summary>
    public interface IMachineMethodData : IMachineMethod
    {
        /// <summary>
        ///     ��ȡ����
        /// </summary>
        /// <returns>���豸��ȡ������</returns>
        Dictionary<string, ReturnUnit> GetDatas(MachineDataType getDataType);

        /// <summary>
        ///     ��ȡ����
        /// </summary>
        /// <returns>���豸��ȡ������</returns>
        Task<Dictionary<string, ReturnUnit>> GetDatasAsync(MachineDataType getDataType);

        /// <summary>
        ///     д������
        /// </summary>
        /// <param name="setDataType">д������</param>
        /// <param name="values">��Ҫд��������ֵ䣬��д������ΪAddressʱ����Ϊ��Ҫд��ĵ�ַ����д������ΪCommunicationTagʱ����Ϊ��Ҫд��ĵ�Ԫ������</param>
        /// <returns>�Ƿ�д��ɹ�</returns>
        bool SetDatas(MachineDataType setDataType, Dictionary<string, double> values);

        /// <summary>
        ///     д������
        /// </summary>
        /// <param name="setDataType">д������</param>
        /// <param name="values">��Ҫд��������ֵ䣬��д������ΪAddressʱ����Ϊ��Ҫд��ĵ�ַ����д������ΪCommunicationTagʱ����Ϊ��Ҫд��ĵ�Ԫ������</param>
        /// <returns>�Ƿ�д��ɹ�</returns>
        Task<bool> SetDatasAsync(MachineDataType setDataType, Dictionary<string, double> values);
    }
}