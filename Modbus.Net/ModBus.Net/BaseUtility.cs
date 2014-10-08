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
        public abstract byte[] GetDatas(byte belongAddress, byte functionCode, string startAddress, ushort getCount);
        public abstract bool SetDatas(byte belongAddress, byte functionCode, string startAddress, object[] setContents);
    }
}
