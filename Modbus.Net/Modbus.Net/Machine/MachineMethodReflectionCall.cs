using System;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     设备的反射调用接口
    /// </summary>
    public static class MachineMethodReflectionCall
    {
        /// <summary>
        ///     反射方式调用获取方法
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="machineMethod">设备方法组</param>
        /// <param name="functionName">方法名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据</returns>
        public static Task<ReturnStruct<T>> InvokeGet<T>(this IMachineMethod machineMethod, string functionName, object[] parameters)
        {
            var machineMethodType = machineMethod.GetType();
            var machineGetMethod = machineMethodType.GetMethod("Get" + functionName + "Async");
            var ans = machineGetMethod.Invoke(machineMethod, parameters);
            return (Task<ReturnStruct<T>>)ans;
        }

        /// <summary>
        ///     反射方式调用设置方法
        /// </summary>
        /// <typeparam name="T">要设置的数据类型</typeparam>
        /// <param name="machineMethod">设备方法组</param>
        /// <param name="functionName">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="datas">要设置的数据</param>
        /// <returns>设置是否成功</returns>
        public static Task<ReturnStruct<bool>> InvokeSet<T>(this IMachineMethod machineMethod, string functionName, object[] parameters, T datas)
        {
            var machineMethodType = machineMethod.GetType();
            var machineSetMethod = machineMethodType.GetMethod("Set" + functionName + "Async");
            object[] allParams = new object[parameters.Length + 1];
            Array.Copy(parameters, allParams, parameters.Length);
            allParams[parameters.Length] = datas;
            var ans = machineSetMethod.Invoke(machineMethod, allParams);
            return (Task<ReturnStruct<bool>>)ans;
        }
    }
}
