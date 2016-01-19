using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public enum MachineSetDataType
    {
        Address,
        CommunicationTag
    }

    public abstract class BaseMachine : IMachineProperty
    {
        public int Id { get; set; }

        public string ProjectName { get; set; }

        public string MachineName { get; set; }

        public bool IsConnected
        {
            get
            {
                return BaseUtility.IsConnected;
            }
        }

        public string ConnectionToken
        {
            get { return BaseUtility.ConnectionToken; }
        }

        public AddressFormater AddressFormater { get; set; }

        public AddressCombiner AddressCombiner { get; set; }

        public AddressTranslator AddressTranslator
        {
            get { return BaseUtility.AddressTranslator; }
            set { BaseUtility.AddressTranslator = value; }
        }

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

        public Dictionary<string, ReturnUnit> GetDatas()
        {
            return AsyncHelper.RunSync(GetDatasAsync);
        }

        public async Task<Dictionary<string,ReturnUnit>> GetDatasAsync()
        {
            try
            {
                Dictionary<string, ReturnUnit> ans = new Dictionary<string, ReturnUnit>();
                if (!BaseUtility.IsConnected)
                {
                    await BaseUtility.ConnectAsync();
                }
                if (!BaseUtility.IsConnected) return null;
                foreach (var communicateAddress in CommunicateAddresses)
                {
                    var datas =
                        await
                            BaseUtility.GetDatasAsync<byte>(2, 0,
                                AddressFormater.FormatAddress(communicateAddress.Area, communicateAddress.Address),
                                (int)
                                    Math.Ceiling(communicateAddress.GetCount*
                                                 BigEndianValueHelper.Instance.ByteLength[
                                                     communicateAddress.DataType.FullName]));
                    if (datas == null || datas.Length == 0) return null;
                    int pos = 0;
                    while (pos < communicateAddress.GetCount)
                    {
                        var address =
                            GetAddresses.SingleOrDefault(
                                p => p.Area == communicateAddress.Area && p.Address == pos + communicateAddress.Address);
                        if (address != null)
                        {
                            ans.Add(address.CommunicationTag,
                                new ReturnUnit
                                {
                                    PlcValue =
                                        Double.Parse(
                                            BigEndianValueHelper.Instance.GetValue(datas, ref pos, address.DataType)
                                                .ToString())*address.Zoom,
                                    UnitExtend = address.UnitExtend
                                });
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
            catch (Exception e)
            {
                Console.WriteLine(ConnectionToken + " " + e.Message);
                return null;
            }
        }

        public bool SetDatas(MachineSetDataType setDataType, Dictionary<string, double> values)
        {
            return AsyncHelper.RunSync(() => SetDatasAsync(setDataType, values));
        }

        public async Task<bool> SetDatasAsync(MachineSetDataType setDataType, Dictionary<string, double> values)
        {
            try
            {
                if (!BaseUtility.IsConnected)
                {
                    await BaseUtility.ConnectAsync();
                }
                if (!BaseUtility.IsConnected) return false;
                List<AddressUnit> addresses = new List<AddressUnit>();
                foreach (var value in values)
                {
                    AddressUnit address = null;
                    switch (setDataType)
                    {
                        case MachineSetDataType.Address:
                        {
                            address =
                                GetAddresses.SingleOrDefault(p => AddressFormater.FormatAddress(p.Area, p.Address) == value.Key);
                            break;
                        }
                        case MachineSetDataType.CommunicationTag:
                        {
                            address =
                                GetAddresses.SingleOrDefault(p => p.CommunicationTag == value.Key);
                            break;
                        }
                    }
                    if (address == null) return false;
                    addresses.Add(address);
                }
                var communcationUnits = AddressCombiner.Combine(addresses);
                foreach (var communicateAddress in communcationUnits)
                {
                    List<object> datasList = new List<object>();
                    var setCount = (int)
                        Math.Ceiling(communicateAddress.GetCount*
                                     BigEndianValueHelper.Instance.ByteLength[
                                         communicateAddress.DataType.FullName]);
                    var allBytes = setCount;

                    while (setCount > 0)
                    {
                        var address = AddressFormater.FormatAddress(communicateAddress.Area,
                            communicateAddress.Address + allBytes - setCount);
                        var addressUnit =
                            GetAddresses.SingleOrDefault(
                                p =>
                                    p.Area == communicateAddress.Area &&
                                    p.Address == communicateAddress.Address + allBytes - setCount);
                        Type dataType = addressUnit.DataType;
                        switch (setDataType)
                        {
                            case MachineSetDataType.Address:
                            {
                                var value = values.SingleOrDefault(p => p.Key == address);
                                await
                                    BaseUtility.SetDatasAsync(2, 0, address,
                                        new object[] {Convert.ChangeType(value.Value, dataType)});
                                break;
                            }
                            case MachineSetDataType.CommunicationTag:
                            {
                                var value = values.SingleOrDefault(p => p.Key == addressUnit.CommunicationTag);
                                await
                                    BaseUtility.SetDatasAsync(2, 0, address,
                                        new object[] {Convert.ChangeType(value.Value, dataType)});
                                break;
                            }
                        }
                        setCount -= (int) BigEndianValueHelper.Instance.ByteLength[dataType.FullName];
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(ConnectionToken + " " + e.Message);
                return false;
            }
            return true;
        }

        public bool Connect()
        {
            return BaseUtility.Connect();
        }

        public async Task<bool> ConnectAsync()
        {
            return await BaseUtility.ConnectAsync();
        }

        public bool Disconnect()
        {
            return BaseUtility.Disconnect();
        }

        public static Dictionary<string, double> MapGetValuesToSetValues(Dictionary<string, ReturnUnit> getValues)
        {
            return (from getValue in getValues
                select new KeyValuePair<string, double>(getValue.Key, getValue.Value.PlcValue)).ToDictionary(p=>p.Key,p=>p.Value);
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
