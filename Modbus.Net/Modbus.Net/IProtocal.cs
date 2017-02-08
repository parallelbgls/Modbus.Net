using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public interface IProtocal
    {
        /// <summary>
        ///     发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="isLittleEndian">是否是小端格式</param>
        /// <param name="content">写入的内容，使用对象数组描述</param>
        /// <returns>从设备获取的字节流</returns>
        byte[] SendReceive(bool isLittleEndian, params object[] content);

        /// <summary>
        ///     发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="isLittleEndian">是否是小端格式</param>
        /// <param name="content">写入的内容，使用对象数组描述</param>
        /// <returns>从设备获取的字节流</returns>
        Task<byte[]> SendReceiveAsync(bool isLittleEndian, params object[] content);

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        IOutputStruct SendReceive(ProtocalUnit unit, IInputStruct content);

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        Task<IOutputStruct> SendReceiveAsync(ProtocalUnit unit, IInputStruct content);
    }
}
