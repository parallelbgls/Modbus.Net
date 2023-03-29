using System;
using System.Collections.Generic;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子设备
    /// </summary>
    public class SiemensMachine<TKey, TUnitKey> : BaseMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备id号</param>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="model">设备类型</param>
        /// <param name="getAddresses">读写的地址</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="src">本机模块位，0到7，仅200使用，其它型号不要填写</param>
        /// <param name="dst">PLC模块位，0到7，仅200使用，其它型号不要填写</param>
        public SiemensMachine(TKey id, SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress, byte src = 1, byte dst = 0)
            : base(id, getAddresses, keepConnect, slaveAddress, masterAddress)
        {
            BaseUtility = new SiemensUtility(connectionType, connectionString, model, slaveAddress, masterAddress, src, dst);
            AddressFormater = new AddressFormaterSiemens();
            AddressCombiner = new AddressCombinerContinus<TUnitKey>(AddressTranslator, 100);
            AddressCombinerSet = new AddressCombinerContinus<TUnitKey>(AddressTranslator, 100);
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备id号</param>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="model">设备类型</param>
        /// <param name="getAddresses">读写的地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="src">本机模块位，0到7，仅200使用，其它型号不要填写</param>
        /// <param name="dst">PLC模块位，0到7，仅200使用，其它型号不要填写</param>
        public SiemensMachine(TKey id, SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit<TUnitKey>> getAddresses, byte slaveAddress, byte masterAddress, byte src = 1, byte dst = 0)
            : this(id, connectionType, connectionString, model, getAddresses, true, slaveAddress, masterAddress, src, dst)
        {
        }
    }
}