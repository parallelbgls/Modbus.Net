﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     基础Api入口
    /// </summary>
    public abstract class BaseUtility<TParamIn, TParamOut, TProtocolUnit, TPipeUnit> : IUtility
        where TProtocolUnit : class, IProtocolFormatting<TParamIn, TParamOut> where TParamOut : class
        where TPipeUnit : PipeUnit<TParamIn, TParamOut, IProtocolLinker<TParamIn, TParamOut>, TProtocolUnit>
    {
        private static readonly ILogger<BaseUtility<TParamIn, TParamOut, TProtocolUnit, TPipeUnit>> logger = LogProvider.CreateLogger<BaseUtility<TParamIn, TParamOut, TProtocolUnit, TPipeUnit>>();

        /// <summary>
        ///     协议收发主体
        /// </summary>
        protected IProtocol<TParamIn, TParamOut, TProtocolUnit, TPipeUnit> Wrapper;

        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseUtility(byte slaveAddress, byte masterAddress)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
            AddressTranslator = new AddressTranslatorBase();
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        protected string ConnectionString { get; set; }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; set; }

        /// <summary>
        ///     主站号
        /// </summary>
        public byte MasterAddress { get; set; }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <param name="getOriginalCount">获取原始个数</param>
        /// <returns>接收到的byte数据</returns>
        public abstract Task<ReturnStruct<byte[]>> GetDatasAsync(string startAddress, int getByteCount, int getOriginalCount);

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCount">获取类型和个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual async Task<ReturnStruct<object[]>> GetDatasAsync(string startAddress,
            KeyValuePair<Type, int> getTypeAndCount)
        {
            try
            {
                var typeName = getTypeAndCount.Key.FullName;
                var bCount = ValueHelper.ByteLength[typeName];
                var getReturnValue = await GetDatasAsync(startAddress,
                    (int)Math.Ceiling(bCount * getTypeAndCount.Value), getTypeAndCount.Value);
                var getBytes = getReturnValue;
                if (getBytes.IsSuccess == false || getBytes.Datas == null)
                {
                    return new ReturnStruct<object[]>
                    {
                        Datas = null,
                        IsSuccess = getBytes.IsSuccess,
                        ErrorCode = getBytes.ErrorCode,
                        ErrorMsg = getBytes.ErrorMsg
                    };
                }
                return new ReturnStruct<object[]>
                {
                    Datas = ValueHelper.GetInstance(Endian).ByteArrayToObjectArray(getBytes.Datas, getTypeAndCount),
                    IsSuccess = getBytes.IsSuccess,
                    ErrorCode = getBytes.ErrorCode,
                    ErrorMsg = getBytes.ErrorMsg
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"ModbusUtility -> GetDatas: {ConnectionString} error");
                return new ReturnStruct<object[]>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = "Unknown Error"
                };
            }
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="T">需要接收的类型</typeparam>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getCount">获取个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual async Task<ReturnStruct<T[]>> GetDatasAsync<T>(string startAddress,
            int getCount)
        {
            try
            {
                var getBytes = await GetDatasAsync(startAddress,
                    new KeyValuePair<Type, int>(typeof(T), getCount));
                if (getBytes.IsSuccess == false || getBytes.Datas == null)
                {
                    return new ReturnStruct<T[]>
                    {
                        Datas = null,
                        IsSuccess = getBytes.IsSuccess,
                        ErrorCode = getBytes.ErrorCode,
                        ErrorMsg = getBytes.ErrorMsg
                    };
                }
                return new ReturnStruct<T[]>
                {
                    Datas = ValueHelper.GetInstance(Endian).ObjectArrayToDestinationArray<T>(getBytes.Datas),
                    IsSuccess = getBytes.IsSuccess,
                    ErrorCode = getBytes.ErrorCode,
                    ErrorMsg = getBytes.ErrorMsg
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"ModbusUtility -> GetDatas Generic: {ConnectionString} error");
                return new ReturnStruct<T[]>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = "Unknown Error"
                };
            }
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCountList">获取类型和个数的队列</param>
        public virtual async Task<ReturnStruct<object[]>> GetDatasAsync(string startAddress,
            IEnumerable<KeyValuePair<Type, int>> getTypeAndCountList)
        {
            try
            {
                var translateTypeAndCount = getTypeAndCountList as IList<KeyValuePair<Type, int>> ??
                                            getTypeAndCountList.ToList();
                var bAllCount = (
                    from getTypeAndCount in translateTypeAndCount
                    let typeName = getTypeAndCount.Key.FullName
                    let bCount = ValueHelper.ByteLength[typeName]
                    select (int)Math.Ceiling(bCount * getTypeAndCount.Value)).Sum();
                var getReturnValue = await GetDatasAsync(startAddress, bAllCount, bAllCount);
                var getBytes = getReturnValue;
                if (getBytes.IsSuccess == false || getBytes.Datas == null)
                {
                    return new ReturnStruct<object[]>
                    {
                        Datas = null,
                        IsSuccess = getBytes.IsSuccess,
                        ErrorCode = getBytes.ErrorCode,
                        ErrorMsg = getBytes.ErrorMsg
                    };
                }
                return new ReturnStruct<object[]>
                {
                    Datas = ValueHelper.GetInstance(Endian).ByteArrayToObjectArray(getBytes.Datas, translateTypeAndCount),
                    IsSuccess = getBytes.IsSuccess,
                    ErrorCode = getBytes.ErrorCode,
                    ErrorMsg = getBytes.ErrorMsg
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"ModbusUtility -> GetDatas pair: {ConnectionString} error");
                return new ReturnStruct<object[]>
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = "Unknown Error"
                };
            }
        }

        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <param name="setOriginalCount">设置原始长度</param>
        /// <returns>是否设置成功</returns>
        public abstract Task<ReturnStruct<bool>> SetDatasAsync(string startAddress, object[] setContents, int setOriginalCount);

        /// <summary>
        ///     协议是否遵循小端格式
        /// </summary>
        public abstract Endian Endian { get; }

        /// <summary>
        ///     设备是否已经连接
        /// </summary>
        public bool IsConnected => Wrapper?.ProtocolLinker != null && Wrapper.ProtocolLinker.IsConnected;

        /// <summary>
        ///     标识设备的连接关键字
        /// </summary>
        public string ConnectionToken
            => Wrapper?.ProtocolLinker == null ? ConnectionString : Wrapper.ProtocolLinker.ConnectionToken;

        /// <summary>
        ///     地址翻译器
        /// </summary>
        public AddressTranslator AddressTranslator { get; set; }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            return await Wrapper.ConnectAsync();
        }

        /// <summary>
        ///     断开设备
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        public bool Disconnect()
        {
            return Wrapper.Disconnect();
        }

        /// <summary>
        ///     返回Utility的方法集合
        /// </summary>
        /// <typeparam name="TUtilityMethod">Utility方法集合类型</typeparam>
        /// <returns>Utility方法集合</returns>
        public TUtilityMethod GetUtilityMethods<TUtilityMethod>() where TUtilityMethod : class, IUtilityMethod
        {
            if (this is TUtilityMethod)
            {
                return this as TUtilityMethod;
            }
            return null;
        }

        /// <summary>
        ///     设置连接类型
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        public abstract void SetConnectionType(int connectionType);
    }
}