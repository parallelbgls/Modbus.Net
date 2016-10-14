using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public static class BaseMachineExtend
    {
        /// <summary>
        /// 将获取的数据转换成可以向设备写入的数据格式
        /// </summary>
        /// <param name="getValues">获取的数据</param>
        /// <returns>写入的数据</returns>
        public static Dictionary<string, double> MapGetValuesToSetValues(this Dictionary<string, ReturnUnit> getValues)
        {
            if (getValues == null) return null;
            return (from getValue in getValues
                where getValue.Value.PlcValue != null
                select new KeyValuePair<string, double>(getValue.Key, getValue.Value.PlcValue.Value)).ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
