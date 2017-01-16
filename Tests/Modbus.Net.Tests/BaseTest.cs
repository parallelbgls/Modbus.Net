using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Tests
{
    public sealed class TestAddresses
    {
        private Dictionary<string, Tuple<int, int, int>[]> Addresses { get; } = new Dictionary<string, Tuple<int, int, int>[]>();

        public Tuple<int, int, int>[] this[string index]
        {
            get { return Addresses[index]; }
            set
            {
                if (!Addresses.ContainsKey(index))
                {
                    Addresses.Add(index, value);
                }
                else
                {
                    Addresses[index] = value;
                }
            }
        }
    }

    public sealed class TestAreas
    {
        private Dictionary<string, string[]> Areas { get; } = new Dictionary<string, string[]>();

        public string[] this[string index]
        {
            get { return Areas[index]; }
            set
            {
                if (!Areas.ContainsKey(index))
                {
                    Areas.Add(index, value);
                }
                else
                {
                    Areas[index] = value;
                }
            }
        }
    }
}
