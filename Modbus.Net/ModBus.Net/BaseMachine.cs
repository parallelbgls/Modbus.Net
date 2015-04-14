using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public abstract class BaseMachine
    {
        public IEnumerable<CommunicationUnit> GetAddresses { get; set; }

        public bool KeepConnect { get; set; }

        public BaseUtility BaseUtility { get; protected set; }

        protected BaseMachine(IEnumerable<CommunicationUnit> getAddresses) : this(getAddresses, false)
        {
        }

        protected BaseMachine(IEnumerable<CommunicationUnit> getAddresses, bool keepConnect)
        {
            GetAddresses = getAddresses;
            KeepConnect = keepConnect;
        }

        public IEnumerable<IEnumerable<object>> GetDatas()
        {
            List<object[]> ans = new List<object[]>();
            if (!BaseUtility.IsConnected)
            {
                BaseUtility.Connect();
            }
            foreach (var getAddress in GetAddresses)
            {
                ans.Add(BaseUtility.GetDatas(2, 0, getAddress.StartAddress, new KeyValuePair<Type, int>(getAddress.DataType, getAddress.GetCount)));
            }
            if (!KeepConnect)
            {
                BaseUtility.DisConnect();
            }
            return ans;
        }
    }

    public struct CommunicationUnit
    {
        public string StartAddress { get; set; }
        public int GetCount { get; set; }
        public Type DataType { get; set; }
    }
}
