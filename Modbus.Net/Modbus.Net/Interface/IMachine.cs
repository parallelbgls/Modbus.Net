using System;

namespace Modbus.Net
{
    public interface IMachine : IMachine<string>
    {
    }

    public interface IMachine<TKey> : IMachineProperty<TKey>, IMachineMethodData where TKey : IEquatable<TKey>
    {
    }
}
