using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc UA设备
    /// </summary>
    /// <typeparam name="TKey">设备Id的类型</typeparam>
    /// <typeparam name="TUnitKey">设备中地址的Id的类型</typeparam>
    public class OpcUaMachine<TKey, TUnitKey> : OpcMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的数据</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcUaMachine(TKey id, string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect, bool isRegexOn = false)
            : base(id, getAddresses, keepConnect)
        {
            BaseUtility = new OpcUaUtility(connectionString, isRegexOn);
            ((OpcUtility) BaseUtility).GetSeperator +=
                () => ((AddressFormaterOpc<string, string>) AddressFormater).Seperator;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的数据</param>
        public OpcUaMachine(TKey id, string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses)
            : this(id, connectionString, getAddresses, false)
        {
        }
    }

    /// <summary>
    ///     Opc UA设备
    /// </summary>
    public class OpcUaMachine : OpcMachine
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的数据</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        public OpcUaMachine(string id, string connectionString, IEnumerable<AddressUnit> getAddresses, bool keepConnect, bool isRegexOn = false)
            : base(id, getAddresses, keepConnect)
        {
            BaseUtility = new OpcUaUtility(connectionString, isRegexOn);
            ((OpcUtility) BaseUtility).GetSeperator +=
                () => ((AddressFormaterOpc<string, string>) AddressFormater).Seperator;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的数据</param>
        public OpcUaMachine(string id, string connectionString, IEnumerable<AddressUnit> getAddresses)
            : this(id, connectionString, getAddresses, false)
        {
        }
    }
}