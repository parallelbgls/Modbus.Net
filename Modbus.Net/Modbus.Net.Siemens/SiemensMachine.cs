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
        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress)
            : base(getAddresses, keepConnect, slaveAddress, masterAddress)
        {
            BaseUtility = new SiemensUtility(connectionType, connectionString, model, slaveAddress, masterAddress);
            AddressFormater = new AddressFormaterSiemens();
            AddressCombiner = new AddressCombinerContinus<TUnitKey>(AddressTranslator);
            AddressCombinerSet = new AddressCombinerContinus<TUnitKey>(AddressTranslator);
        }

        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit<TUnitKey>> getAddresses, byte slaveAddress, byte masterAddress)
            : this(connectionType, connectionString, model, getAddresses, false, slaveAddress, masterAddress)
        {
        }
    }

    /// <summary>
    ///     西门子设备
    /// </summary>
    public class SiemensMachine : SiemensMachine<string, string>
    {
        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit<string>> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress)
            : base(connectionType, connectionString, model, getAddresses, keepConnect, slaveAddress, masterAddress)
        {
        }

        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit<string>> getAddresses, byte slaveAddress, byte masterAddress)
            : base(connectionType, connectionString, model, getAddresses, slaveAddress, masterAddress)
        {
        }
    }
}