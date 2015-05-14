using System;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    public abstract class BaseMachine : IMachineProperty
    {
        public string Id { get; set; }

        public string ProjectName { get; set; }

        public string MachineName { get; set; }

        public string ConnectionToken
        {
            get { return BaseUtility.ConnectionToken; }
        }

        public AddressFormater AddressFormater { get; set; }

        public AddressCombiner AddressCombiner { get; set; }

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

        public Dictionary<string,string> GetDatas()
        {
            Dictionary<string, string> ans = new Dictionary<string, string>();
            if (!BaseUtility.IsConnected)
            {
                BaseUtility.Connect();
            }
            foreach (var communicateAddress in CommunicateAddresses)
            {
                var datas = BaseUtility.GetDatas<byte>(2, 0, AddressFormater.FormatAddress(communicateAddress.Area,communicateAddress.Address), communicateAddress.GetCount);
                int pos = 0;
                while (pos < communicateAddress.GetCount)
                {
                    var address =
                        GetAddresses.SingleOrDefault(
                            p => p.Area == communicateAddress.Area && p.Address == pos + communicateAddress.Address);
                    if (address != null)
                    {
                        ans.Add(address.CommunicationTag, (String.Format("{0:#0.#}", Math.Round(Double.Parse(ValueHelper.Instance.GetValue(datas, ref pos, address.DataType).ToString()) * address.Zoom, 3))));
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

    public struct CommunicationUnit
    {
        public string Area { get; set; }
        public int Address { get; set; }
        public int GetCount { get; set; }
        public Type DataType { get; set; }
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
        /// <summary>
        /// 通讯标识名称
        /// </summary>
        public string CommunicationTag { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
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
        string Id { get; set; }
        string ProjectName { get; set; }
        string MachineName { get; set; }
        string ConnectionToken { get; }
    }
}
