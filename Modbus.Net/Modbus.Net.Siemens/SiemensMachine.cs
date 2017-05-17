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
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="model">设备类型</param>
        /// <param name="getAddresses">读写的地址</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress)
            : base(getAddresses, keepConnect, slaveAddress, masterAddress)
        {
            BaseUtility = new SiemensUtility(connectionType, connectionString, model, slaveAddress, masterAddress);
            AddressFormater = new AddressFormaterSiemens();
            AddressCombiner = new AddressCombinerContinus<TUnitKey>(AddressTranslator, 100);
            AddressCombinerSet = new AddressCombinerContinus<TUnitKey>(AddressTranslator, 100);
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="model">设备类型</param>
        /// <param name="getAddresses">读写的地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit<TUnitKey>> getAddresses, byte slaveAddress, byte masterAddress)
            : this(connectionType, connectionString, model, getAddresses, false, slaveAddress, masterAddress)
        {
        }
    }

    /// <summary>
    ///     西门子设备
    /// </summary>
    public class SiemensMachine : BaseMachine
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="model">设备类型</param>
        /// <param name="getAddresses">读写的地址</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress)
            : base(getAddresses, keepConnect, slaveAddress, masterAddress)
        {
            BaseUtility = new SiemensUtility(connectionType, connectionString, model, slaveAddress, masterAddress);
            AddressFormater = new AddressFormaterSiemens();
            AddressCombiner = new AddressCombinerContinus(AddressTranslator, 100);
            AddressCombinerSet = new AddressCombinerContinus(AddressTranslator, 100);
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="model">设备类型</param>
        /// <param name="getAddresses">读写的地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit> getAddresses, byte slaveAddress, byte masterAddress)
            : this(connectionType, connectionString, model, getAddresses, false, slaveAddress, masterAddress)
        {
        }
    }
}