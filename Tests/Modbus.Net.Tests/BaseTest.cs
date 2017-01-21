using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Tests
{
    public sealed class TestAddresses
    {
        private Dictionary<string, Tuple<double, int, Type>[]> Addresses { get; }

        public Tuple<double, int, Type>[] this[string index]
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

        public TestAddresses(Dictionary<string, Tuple<double, int, Type>[]> addresses)
        {
            Addresses = addresses.ToDictionary(address=>address.Key, address=>address.Value);
        }

        public IEnumerable<string> Keys => Addresses.Keys.AsEnumerable();

        public IEnumerable<Tuple<double, int, Type>[]> Values => Addresses.Values.AsEnumerable();
    }

    public sealed class TestAreas
    {
        private Dictionary<string, string[]> Areas { get; }

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

        public TestAreas(Dictionary<string, string[]> addresses)
        {
            Areas = addresses.ToDictionary(address => address.Key, address => address.Value);
        }

        public IEnumerable<string> Keys => Areas.Keys.AsEnumerable();

        public IEnumerable<string[]> Values => Areas.Values.AsEnumerable();
    }

    public sealed class BaseTest
    {
        public static TestAreas TestAreasModbus => new TestAreas(new Dictionary<string, string[]>
        {
            {
                "Coil",new []{"0X", "1X"}
            },
            {
                "Register", new [] {"3X", "4X"}
            }
        });

        public static TestAddresses TestAddresses => new TestAddresses(new Dictionary<string, Tuple<double, int, Type>[]>
        {
            {
                "Coil.Single.Min", new []
                {
                    new Tuple<double, int, Type>(0, 1, typeof(bool))
                }
            },
            { 
                "Coil.Single.Normal", new[]
                {
                    new Tuple<double, int, Type>(100, 1, typeof(bool))
                }
            },
            {
                "Coil.Single.MaxOverFlow", new[]
                {
                    new Tuple<double, int, Type>(100000, 1, typeof(bool))
                }
            },
            {
                "Coil.Multi.Normal", new[]
                {
                    new Tuple<double, int, Type>(0, 30, typeof(bool))
                }
            },
            {
                "Register.Single.Short", new[]
                {
                    new Tuple<double, int, Type>(0, 1, typeof(ushort))
                }
            },
            {
                "Register.Continus.Short", new[]
                {
                    new Tuple<double, int, Type>(0, 10, typeof(ushort))
                }
            },
            {
                "Register.Continus.Byte", new[]
                {
                    new Tuple<double, int, Type>(0, 10, typeof(byte))
                }
            },
            {
                "Register.Continus.Int", new[]
                {
                    new Tuple<double, int, Type>(0, 10, typeof(uint))
                }
            },
            {
                "Register.Continus.Bit", new []
                {
                    new Tuple<double, int, Type>(0.5, 8, typeof(bool))
                }
            },
            {
                "Register.Duplicate.Short", new []
                {
                    new Tuple<double, int, Type>(0, 10, typeof(ushort)),
                    new Tuple<double, int, Type>(15, 25, typeof(ushort)),
                    new Tuple<double, int, Type>(50, 20, typeof(ushort))
                }
            },
            {
                "Register.Cross.Short", new []
                {
                    new Tuple<double, int, Type>(0, 10, typeof(ushort)),
                    new Tuple<double, int, Type>(5, 10, typeof(ushort)),
                    new Tuple<double, int, Type>(10, 10, typeof(ushort))
                }
            },
            {
                "Register.Duplicate.Multi", new []
                {
                    new Tuple<double, int, Type>(0, 10, typeof(byte)),
                    new Tuple<double, int, Type>(20, 10, typeof(byte)),
                    new Tuple<double, int, Type>(30, 16, typeof(bool))
                }
            },
        });
    }
}
