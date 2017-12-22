using System.Collections.Generic;
using System.Configuration;

namespace Modbus.Net.OPC.FBox
{
    /// <summary>
    ///     FBox的Opc服务设备
    /// </summary>
    public class FBoxOpcDaMachine : OpcDaMachine
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="localSequence">页名称</param>
        /// <param name="linkerName">设备名称</param>
        /// <param name="getAddresses">获取地址</param>
        /// <param name="keepConnect">是否保持连接</param>
        public FBoxOpcDaMachine(string id, string localSequence, string linkerName,
            IEnumerable<AddressUnit> getAddresses, bool keepConnect)
            : base(id,
                ConfigurationManager.AppSettings["FBoxOpcDaHost"] ?? "opcda://localhost/FBoxOpcServer", getAddresses,
                keepConnect, true)
        {
            LocalSequence = localSequence;
            LinkerName = linkerName;
            AddressFormater =
                new AddressFormaterOpc<string, string>(
                    (machine, unit) =>
                        new[]
                        {
                            "(.*)", ((FBoxOpcDaMachine) machine).LinkerName, ((FBoxOpcDaMachine) machine).LocalSequence,
                            unit.Name
                        }, this, '.');
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="localSequence">页名称</param>
        /// <param name="linkerName">设备名称</param>
        /// <param name="getAddresses">获取地址</param>
        public FBoxOpcDaMachine(string id, string localSequence, string linkerName,
            IEnumerable<AddressUnit> getAddresses)
            : this(id, localSequence, linkerName, getAddresses, false)
        {
        }

        /// <summary>
        ///     页名称
        /// </summary>
        public string LocalSequence { get; set; }

        /// <summary>
        ///     设备名称
        /// </summary>
        public string LinkerName { get; set; }
    }
}