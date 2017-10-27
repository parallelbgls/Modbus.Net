using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc DA设备
    /// </summary>
    /// <typeparam name="TKey">设备Id类型</typeparam>
    /// <typeparam name="TUnitKey">设备包含的地址的Id类型</typeparam>
    public class OpcDaMachine<TKey, TUnitKey> : OpcMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的数据</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcDaMachine(TKey id, string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect, bool isRegexOn = false)
            : base(id, getAddresses, keepConnect)
        {
            BaseUtility = new OpcDaUtility(connectionString, isRegexOn);
            ((OpcUtility) BaseUtility).GetSeperator +=
                () => ((AddressFormaterOpc<TKey, TUnitKey>) AddressFormater).Seperator;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的数据</param>
        public OpcDaMachine(TKey id, string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses)
            : this(id, connectionString, getAddresses, false)
        {
        }
    }

    /// <summary>
    ///     Opc DA设备
    /// </summary>
    public class OpcDaMachine : OpcMachine
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的数据</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcDaMachine(string id, string connectionString, IEnumerable<AddressUnit> getAddresses, bool keepConnect, bool isRegexOn = false)
            : base(id, getAddresses, keepConnect)
        {
            BaseUtility = new OpcDaUtility(connectionString, isRegexOn);
            ((OpcUtility) BaseUtility).GetSeperator +=
                () => ((AddressFormaterOpc<string, string>) AddressFormater).Seperator;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的数据</param>
        public OpcDaMachine(string id, string connectionString, IEnumerable<AddressUnit> getAddresses)
            : this(id, connectionString, getAddresses, false)
        {
        }
    }
}