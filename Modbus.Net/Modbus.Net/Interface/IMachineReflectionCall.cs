using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     设备的反射调用接口
    /// </summary>
    public interface IMachineReflectionCall
    {
        /// <summary>
        ///     反射方式调用获取方法
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="functionName">方法名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据</returns>
        Task<ReturnStruct<T>> InvokeGet<T>(string functionName, object[] parameters);

        /// <summary>
        ///     反射方式调用设置方法
        /// </summary>
        /// <typeparam name="T">要设置的数据类型</typeparam>
        /// <param name="functionName">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="datas">要设置的数据</param>
        /// <returns>设置是否成功</returns>
        Task<ReturnStruct<bool>> InvokeSet<T>(string functionName, object[] parameters, T datas);
    }
}
