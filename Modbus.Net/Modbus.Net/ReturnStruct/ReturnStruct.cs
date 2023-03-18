using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     返回引用类型
    /// </summary>
    /// <typeparam name="TDataType"></typeparam>
    public struct ReturnStruct<TDataType>
    {
        /// <summary>
        ///     数据
        /// </summary>
        public TDataType Datas { get; set; }
        /// <summary>
        ///     操作是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        ///     错误代码
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        ///     错误详细信息
        /// </summary>
        public string ErrorMsg { get; set; }
    }
}
