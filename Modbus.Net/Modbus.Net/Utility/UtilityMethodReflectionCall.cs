using System;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     设备的反射调用接口
    /// </summary>
    public static class UtilityMethodReflectionCall
    {
        /// <summary>
        ///     反射方式调用获取方法
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="utilityMethod">方法组</param>
        /// <param name="functionName">方法名</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据</returns>
        public static Task<ReturnStruct<T>> InvokeGet<T>(this IUtilityMethod utilityMethod, string functionName, object[] parameters)
        {
            var utilityMethodType = utilityMethod.GetType();
            var utilityGetMethod = utilityMethodType.GetMethod("Get" + functionName + "Async");
            var ans = utilityGetMethod.Invoke(utilityMethod, parameters);
            return (Task<ReturnStruct<T>>)ans;
        }

        /// <summary>
        ///     反射方式调用设置方法
        /// </summary>
        /// <typeparam name="T">要设置的数据类型</typeparam>
        /// <param name="utilityMethod">方法组</param>
        /// <param name="functionName">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="datas">要设置的数据</param>
        /// <returns>设置是否成功</returns>
        public static Task<ReturnStruct<bool>> InvokeSet<T>(this IUtilityMethod utilityMethod, string functionName, object[] parameters, T datas)
        {
            var utilityMethodType = utilityMethod.GetType();
            var utilitySetMethod = utilityMethodType.GetMethod("Set" + functionName + "Async");
            object[] allParams = new object[parameters.Length + 1];
            Array.Copy(parameters, allParams, parameters.Length);
            allParams[parameters.Length] = datas;
            var ans = utilitySetMethod.Invoke(utilityMethod, allParams);
            return (Task<ReturnStruct<bool>>)ans;
        }
    }
}
