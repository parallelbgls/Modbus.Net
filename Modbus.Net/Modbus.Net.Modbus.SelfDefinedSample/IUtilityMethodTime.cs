using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus.SelfDefinedSample
{
    /// <summary>
    ///     Utility的时间读写接口
    /// </summary>
    public interface IUtilityMethodTime : IUtilityMethod
    {
        /// <summary>
        ///     获取PLC时间
        /// </summary>
        /// <returns>PLC时间</returns>
        Task<ReturnStruct<DateTime>> GetTimeAsync();

        /// <summary>
        ///     设置PLC时间
        /// </summary>
        /// <param name="setTime">设置PLC时间</param>
        /// <returns>设置是否成功</returns>
        Task<ReturnStruct<bool>> SetTimeAsync(DateTime setTime);
    }
}
