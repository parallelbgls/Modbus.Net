using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Modbus.Net
{
    /// <summary>
    ///     程序集辅助类
    /// </summary>
    public static class AssemblyHelper
    {
        /// <summary>
        ///     获取与Modbus.Net相关的所有程序集
        /// </summary>
        /// <returns></returns>
        public static List<Assembly> GetAllLibraryAssemblies()
        {
            List<Assembly> allAssemblies = new List<Assembly>();
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            allAssemblies.Add(Assembly.Load("Modbus.Net"));

            foreach (string dll in Directory.GetFiles(path, "Modbus.Net.*.dll"))
                allAssemblies.Add(Assembly.LoadFile(dll));

            return allAssemblies;
        }
    }
}
