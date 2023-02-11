using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Interface
{
    public interface IMachine : IMachine<string>
    {
    }

    public interface IMachine<TKey> : IMachineProperty<TKey>, IMachineMethodData where TKey : IEquatable<TKey>
    {
    }
}
