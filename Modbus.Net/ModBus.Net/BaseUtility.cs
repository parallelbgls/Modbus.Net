using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public abstract class BaseUtility
    {
        public abstract void SetConnectionString(string connectionString);
        public abstract void SetConnectionType(int connectionType);
        public abstract bool[] GetCoils(byte belongAddress, string startAddress, ushort getCount);
        public abstract bool SetCoils(byte belongAddress, string startAddress, bool[] setContents);
        public abstract ushort[] GetRegisters(byte belongAddress, string startAddress, ushort getCount);
        public abstract bool SetRegisters(byte belongAddress, string startAddress, object[] setContents);
    }
}
