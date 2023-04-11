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
        /// <typeparam name="TMachineMethod">方法组的类型</typeparam>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="machineMethod">方法组</param>
        /// <param name="parameters">参数</param>
        /// <returns>返回的数据</returns>
        public static Task<ReturnStruct<T>> InvokeGet<TMachineMethod, T>(this IMachineMethod machineMethod, object[] parameters) where TMachineMethod : IMachineMethod
        {
            if (typeof(TMachineMethod).Name[..14] != "IMachineMethod")
            {
                throw new NotSupportedException("IMachineMethod type name not begin with IMachineMethod");
            }
            var functionName = "Get" + typeof(TMachineMethod).Name[14..] + "Async";
            return InvokeGet<T>(machineMethod, functionName, parameters);
        }

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
            var machineGetMethod = machineMethodType.GetMethod(functionName);
            var ans = machineGetMethod.Invoke(machineMethod, parameters);
            return (Task<ReturnStruct<T>>)ans;
        }

        /// <summary>
        ///     反射方式调用设置方法
        /// </summary>
        /// <typeparam name="TMachineMethod">方法组的类型</typeparam>
        /// <typeparam name="T">要设置的数据类型</typeparam>
        /// <param name="machineMethod">方法组</param>
        /// <param name="parameters">参数</param>
        /// <param name="datas">要设置的数据</param>
        /// <returns>设置是否成功</returns>
        public static Task<ReturnStruct<bool>> InvokeSet<TMachineMethod, T>(this IMachineMethod machineMethod, object[] parameters, T datas) where TMachineMethod : IMachineMethod
        {
            if (typeof(TMachineMethod).Name[..14] != "IMachineMethod")
            {
                throw new NotSupportedException("IMachineMethod type name not begin with IMachineMethod");
            }
            var functionName = "Set" + typeof(TMachineMethod).Name[14..] + "Async";
            return InvokeSet(machineMethod, functionName, parameters, datas);
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
            var machineSetMethod = machineMethodType.GetMethod(functionName);
            object[] allParams = new object[parameters.Length + 1];
            Array.Copy(parameters, allParams, parameters.Length);
            allParams[parameters.Length] = datas;
            var ans = machineSetMethod.Invoke(machineMethod, allParams);
            return (Task<ReturnStruct<bool>>)ans;
        }
    }
}
