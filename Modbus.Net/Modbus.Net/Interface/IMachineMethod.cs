using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     Machine读写方法接口
    /// </summary>
    public interface IMachineMethod
    {
    }

    /// <summary>
    ///     Machine的数据读写接口
    /// </summary>
    public interface IMachineMethodData : IMachineMethod
    {
        /// <summary>
        ///     读取数据
        /// </summary>
        /// <returns>从设备读取的数据</returns>
        Dictionary<string, ReturnUnit> GetDatas(MachineDataType getDataType);

        /// <summary>
        ///     读取数据
        /// </summary>
        /// <returns>从设备读取的数据</returns>
        Task<Dictionary<string, ReturnUnit>> GetDatasAsync(MachineDataType getDataType);

        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="setDataType">写入类型</param>
        /// <param name="values">需要写入的数据字典，当写入类型为Address时，键为需要写入的地址，当写入类型为CommunicationTag时，键为需要写入的单元的描述</param>
        /// <returns>是否写入成功</returns>
        bool SetDatas(MachineDataType setDataType, Dictionary<string, double> values);

        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="setDataType">写入类型</param>
        /// <param name="values">需要写入的数据字典，当写入类型为Address时，键为需要写入的地址，当写入类型为CommunicationTag时，键为需要写入的单元的描述</param>
        /// <returns>是否写入成功</returns>
        Task<bool> SetDatasAsync(MachineDataType setDataType, Dictionary<string, double> values);
    }
}