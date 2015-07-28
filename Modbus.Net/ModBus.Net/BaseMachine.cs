using System;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    public abstract class BaseMachine : IMachineProperty
    {
        public int Id { get; set; }

        public string ProjectName { get; set; }

        public string MachineName { get; set; }

        public string ConnectionToken
        {
            get { return BaseUtility.ConnectionToken; }
        }

        public AddressFormater AddressFormater { get; set; }

        public AddressCombiner AddressCombiner { get; set; }

        public MachineExtend MachineExtend { get; set; }

        protected IEnumerable<CommunicationUnit> CommunicateAddresses
        {
            get { return AddressCombiner.Combine(GetAddresses); }
        }

        public IEnumerable<AddressUnit> GetAddresses { get; set; }

        public bool KeepConnect { get; set; }

        protected BaseUtility BaseUtility { get; set; }

        protected BaseMachine(IEnumerable<AddressUnit> getAddresses)
            : this(getAddresses, false)
        {
        }

        protected BaseMachine(IEnumerable<AddressUnit> getAddresses, bool keepConnect)
        {
            GetAddresses = getAddresses;
            KeepConnect = keepConnect;
        }

        public Dictionary<string,ReturnUnit> GetDatas()
        {
            Dictionary<string, ReturnUnit> ans = new Dictionary<string, ReturnUnit>();
            if (!BaseUtility.IsConnected)
            {
                BaseUtility.Connect();
            }
            foreach (var communicateAddress in CommunicateAddresses)
            {
                var datas = BaseUtility.GetDatas<byte>(2, 0, AddressFormater.FormatAddress(communicateAddress.Area,communicateAddress.Address), communicateAddress.GetCount);
                if (datas == null || datas.Length == 0) return null;
                int pos = 0;
                while (pos < communicateAddress.GetCount)
                {
                    var address =
                        GetAddresses.SingleOrDefault(
                            p => p.Area == communicateAddress.Area && p.Address == pos + communicateAddress.Address);
                    if (address != null)
                    {
                        ans.Add(address.CommunicationTag, new ReturnUnit{PlcValue = Double.Parse(ValueHelper.Instance.GetValue(datas, ref pos, address.DataType).ToString()) * address.Zoom,UnitExtend = address.UnitExtend});
                    }
                    else
                    {
                        pos++;
                    }
                }
            }
            if (!KeepConnect)
            {
                BaseUtility.Disconnect();
            }
            if (ans.Count == 0) ans = null;
            return ans;
        }

        public bool Connect()
        {
            return BaseUtility.Connect();
        }

        public bool Disconnect()
        {
            return BaseUtility.Disconnect();
        }
    }

    public class BaseMachineEqualityComparer : IEqualityComparer<BaseMachine>
    {
        public bool Equals(BaseMachine x, BaseMachine y)
        {
            return x.ConnectionToken == y.ConnectionToken;
        }

        public int GetHashCode(BaseMachine obj)
        {
            return obj.GetHashCode();
        }
    }

    public class CommunicationUnit
    {
        public string Area { get; set; }
        public int Address { get; set; }
        public int GetCount { get; set; }
        public Type DataType { get; set; }
    }

    public class UnitExtend
    {
        
    }

    public class MachineExtend
    {
        
    }

    public class ReturnUnit
    {
        public double PlcValue { get; set; }
        public UnitExtend UnitExtend { get; set; }
    }

    public class AddressUnit
    {
        public int Id { get; set; }
        /// <summary>
        /// 数据所属的区域，Modbus指通讯码，PROFINET指数据块
        /// </summary>
        public string Area { get; set; }
        public int Address { get; set; }
        public Type DataType { get; set; }
        /// <summary>
        /// 放缩比例
        /// </summary>
        public double Zoom { get; set; }
        public int DecimalPos { get; set; }
        /// <summary>
        /// 通讯标识名称
        /// </summary>
        public string CommunicationTag { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }

        public UnitExtend UnitExtend { get; set; }
    }

    public struct AddressUnitEqualityComparer : IEqualityComparer<AddressUnit>
    {
        public bool Equals(AddressUnit x, AddressUnit y)
        {
            return x.Area.ToUpper() == y.Area.ToUpper() && x.Address == y.Address;
        }

        public int GetHashCode(AddressUnit obj)
        {
            return obj.GetHashCode();
        }
    }

    public interface IMachineProperty
    {
        int Id { get; set; }
        string ProjectName { get; set; }
        string MachineName { get; set; }
        string ConnectionToken { get; }
    }
}
