using System;
using System.Collections.Generic;

namespace ModBus.Net.FBox
{
    public class FBoxMachine : BaseMachine
    {
        public FBoxMachine(FBoxType fBoxType, string connectionString, SignalRSigninMsg msg, IEnumerable<AddressUnit> getAddresses, bool keepConnect) : base(getAddresses, keepConnect)
        {
            AddressFormater = new AddressFormaterFBox();
            AddressCombiner = new AddressCombinerFBox();
            BaseUtility = new FBoxUtility(fBoxType, connectionString, CommunicateAddresses, msg);      
        }
    }
}
