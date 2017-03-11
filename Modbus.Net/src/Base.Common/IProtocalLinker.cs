using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public interface IProtocalLinker
    {
        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        byte[] SendReceive(byte[] content);

        /// <summary>
        ///     发送并接收数据
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        Task<byte[]> SendReceiveAsync(byte[] content);

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        byte[] SendReceiveWithoutExtAndDec(byte[] content);

        /// <summary>
        ///     发送并接收数据，不进行协议扩展和收缩，用于特殊协议
        /// </summary>
        /// <param name="content">发送协议的内容</param>
        /// <returns>接收协议的内容</returns>
        Task<byte[]> SendReceiveWithoutExtAndDecAsync(byte[] content);

        /// <summary>
        ///     检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        bool? CheckRight(byte[] content);

        /// <summary>
        ///     协议内容扩展，发送时根据需要扩展
        /// </summary>
        /// <param name="content">扩展前的基本协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        byte[] BytesExtend(byte[] content);

        /// <summary>
        ///     协议内容缩减，接收时根据需要缩减
        /// </summary>
        /// <param name="content">缩减前的完整协议内容</param>
        /// <returns>缩减后的协议内容</returns>
        byte[] BytesDecact(byte[] content);
    }
}
