using System;
using System.Reflection;

namespace Modbus.Net
{
    /// <summary>
    ///     ProtocolLinker添加Controller扩展方法
    /// </summary>
    public static class ControllerHelper
    {
        /// <summary>
        ///     添加一个Controller
        /// </summary>
        /// <param name="protocolLinker">ProtocolLinker实例</param>
        /// <param name="param1">第一参数</param>
        /// <param name="param2">第二参数</param>
        /// <param name="connector">Connector实例</param>
        /// <exception cref="NotImplementedException">如果没有发现控制器，报错</exception>
        public static void AddController(this IProtocolLinker<byte[], byte[]> protocolLinker, string param1, int param2, IConnector<byte[], byte[]> connector)
        {
            IController controller = null;
            var assemblies = AssemblyHelper.GetAllLibraryAssemblies();
            string controllerName = protocolLinker.GetType().Name.Substring(0, protocolLinker.GetType().Name.Length - 14) + "Controller";
            foreach (var assembly in assemblies)
            {
                var controllerType = assembly.GetType(assembly.GetName().Name + "." + controllerName);
                if (controllerType != null)
                {
                    controller = assembly.CreateInstance(controllerType.FullName, true, BindingFlags.Default, null, new object[2] { param1, param2 }, null, null) as IController;
                    break;
                }
            }
            if (controller != null)
            {
                ((IConnectorWithController<byte[], byte[]>)connector).AddController(controller);
                return;
            }
            throw new NotImplementedException(controllerName + " not found exception");
        }
    }
}
