﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     Utility方法读写接口
    /// </summary>
    public interface IUtilityMethod
    {
    }

    /// <summary>
    ///     Utility的数据读写接口
    /// </summary>
    public interface IUtilityMethodDatas : IUtilityMethod
    {
        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <param name="getOriginalCount">获取原始个数</param>
        /// <returns>接收到的byte数据</returns>
        Task<ReturnStruct<byte[]>> GetDatasAsync(string startAddress, int getByteCount, int getOriginalCount);

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCount">获取类型和个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        Task<ReturnStruct<object[]>> GetDatasAsync(string startAddress, KeyValuePair<Type, int> getTypeAndCount);

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="T">需要接收的类型</typeparam>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        Task<ReturnStruct<T[]>> GetDatasAsync<T>(string startAddress, int getByteCount);

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCountList">获取类型和个数的队列</param>
        Task<ReturnStruct<object[]>> GetDatasAsync(string startAddress, IEnumerable<KeyValuePair<Type, int>> getTypeAndCountList);

        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <param name="setOriginalCount">设置原始长度</param>
        /// <returns>是否设置成功</returns>
        Task<ReturnStruct<bool>> SetDatasAsync(string startAddress, object[] setContents, int setOriginalCount);
    }
}