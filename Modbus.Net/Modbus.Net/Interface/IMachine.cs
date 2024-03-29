﻿using System;

namespace Modbus.Net
{
    /// <summary>
    ///     设备的抽象
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IMachine<TKey> : IMachineProperty<TKey>, IMachineMethodDatas where TKey : IEquatable<TKey>
    {
    }
}
