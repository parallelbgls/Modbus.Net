using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     Utility方法读写接口
    /// </summary>
    public interface IUtilityServerMethod
    {
    }

    /// <summary>
    ///     Utility的数据读写接口
    /// </summary>
    public interface IUtilityServerMethodDatas : IUtilityServerMethod
    {
        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的byte数据</returns>
        Task<ReturnStruct<byte[]>> GetServerDatasAsync(string startAddress, int getByteCount);

        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        Task<ReturnStruct<bool>> SetServerDatasAsync(string startAddress, object[] setContents);
    }
}
