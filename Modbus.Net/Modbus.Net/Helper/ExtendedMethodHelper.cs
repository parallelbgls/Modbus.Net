using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Modbus.Net
{
    /// <summary>
    ///     获取扩展方法的类
    /// </summary>
    public static class ExtendedMethodHelper
    {
        /// <summary>
        ///     获取程序集中的所有扩展方法
        /// </summary>
        /// <param name="extendedType">扩展方法的第一个参数类即扩展类</param>
        /// <param name="assembly">程序集</param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type extendedType, Assembly assembly)
        {
            var query = from type in assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static
                            | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == extendedType
                        select method;
            return query.ToList();
        }
    }
}
