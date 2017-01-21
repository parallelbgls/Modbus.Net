using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class CommunicationTest
    {
        protected AddressMaker AddressMaker = new AddressMaker()
        {
            TestAreas = BaseTest.TestAreasModbus,
            TestAddresses = BaseTest.TestAddresses
        };

        protected BaseMachine Machine = new ModbusMachine(ModbusType.Tcp, "192.168.3.12", null, true, 2, 0);

        [TestMethod]
        public async Task CoilSingle()
        {
            var addresses = AddressMaker.MakeAddresses("Coil", "Coil.Single.Normal", false);
            Machine.AddressCombiner = new AddressCombinerNumericJump(20, Machine.AddressTranslator);
            Machine.GetAddresses = addresses;
            var ans = await Machine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "0X 100", 1
                },
                {
                    "1X 100", 1
                }
            });
            var addresses2 = AddressMaker.MakeAddresses("Coil", "Coil.Signle.Normal", true);
            Machine.AddressCombiner = new AddressCombinerNumericJump(20, Machine.AddressTranslator);
            Machine.GetAddresses = addresses2;
            var ans2 = await Machine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans2["0X 100"], 1);
            Assert.AreEqual(ans2["1X 100"], 1);
        }

        [TestMethod]
        public async Task CoilMuiltiRead()
        {
            var addresses = AddressMaker.MakeAddresses("Coil", "Coil.Multi.Normal", false);
            Machine.AddressCombiner = new AddressCombinerNumericJump(20, Machine.AddressTranslator);
            Machine.GetAddresses = addresses;
            var ans = await Machine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "0X 3", 1
                },
                {
                    "0X 4", 1
                },
                {
                    "0X 16", 1
                },
                {
                    "0X 22", 1
                },
                {
                    "1X 3", 1
                },
                {
                    "1X 4", 1
                },
                {
                    "1X 16", 1
                },
                {
                    "1X 22", 1
                }
            });
            var addresses2 = AddressMaker.MakeAddresses("Coil", "Coil.Multi.Normal", true);
            Machine.AddressCombiner = new AddressCombinerNumericJump(20, Machine.AddressTranslator);
            Machine.GetAddresses = addresses;
            var ans2 = await Machine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans2["0X 3"], 1);
            Assert.AreEqual(ans2["0X 4"], 1);
            Assert.AreEqual(ans2["0X 16"], 1);
            Assert.AreEqual(ans2["0X 22"], 1);
            Assert.AreEqual(ans2["0X 7"], 0);
            Assert.AreEqual(ans2["0X 29"], 0);
            Assert.AreEqual(ans2["1X 3"], 1);
            Assert.AreEqual(ans2["1X 4"], 1);
            Assert.AreEqual(ans2["1X 16"], 1);
            Assert.AreEqual(ans2["1X 22"], 1);
            Assert.AreEqual(ans2["1X 7"], 0);
            Assert.AreEqual(ans2["1X 29"], 0);
        }

        [TestMethod]
        public async Task RegisterSingleRead()
        {
            var addresses = AddressMaker.MakeAddresses("Register", "Register.Single.Short", false);
            Machine.AddressCombiner = new AddressCombinerNumericJump(20, Machine.AddressTranslator);
            Machine.GetAddresses = addresses;
            var ans = await Machine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "3X 0", 32767
                },
                {
                    "4X 0", 32767
                }
            });
            var addresses2 = AddressMaker.MakeAddresses("Register", "Register.Single.Short", true);
            Machine.AddressCombiner = new AddressCombinerNumericJump(20, Machine.AddressTranslator);
            Machine.GetAddresses = addresses;
            var ans2 = await Machine.GetDatasAsync(MachineGetDataType.Address);
            
            Assert.AreEqual(ans2["3X 0"], 32767);
            Assert.AreEqual(ans2["4X 0"], 32767);
        }

        [TestMethod]
        public async Task RegistertMultiRead()
        {
            var addresses = AddressMaker.MakeAddresses("Register", "Register.Duplicate.Short", false);
            Machine.AddressCombiner = new AddressCombinerNumericJump(20, Machine.AddressTranslator);
            Machine.GetAddresses = addresses;
            var ans = await Machine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "3X 4", 17
                },
                {
                    "3X 7", 20
                },
                {
                    "3X 17", 5255
                },
                {
                    "3X 18", 3019
                },
                {
                    "3X 55", 192
                },
                {
                    "4X 4", 18
                },
                {
                    "4X 7", 21
                },
                {
                    "4X 17", 5256
                },
                {
                    "4X 18", 3020
                },
                {
                    "4X 55", 193
                }
            });

            var addresses2 = AddressMaker.MakeAddresses("Register", "Register.Duplicate.Short", true);
            Machine.AddressCombiner = new AddressCombinerNumericJump(20, Machine.AddressTranslator);
            Machine.GetAddresses = addresses;
            var ans2 = await Machine.GetDatasAsync(MachineGetDataType.Address);
            
            Assert.AreEqual(ans2["3X 4"], 17);
            Assert.AreEqual(ans2["3X 7"], 20);
            Assert.AreEqual(ans2["3X 17"], 5255);
            Assert.AreEqual(ans2["3X 18"], 3019);
            Assert.AreEqual(ans2["3X 55"], 192);

            Assert.AreEqual(ans2["4X 4"], 18);
            Assert.AreEqual(ans2["4X 7"], 21);
            Assert.AreEqual(ans2["4X 17"], 5256);
            Assert.AreEqual(ans2["4X 18"], 3020);
            Assert.AreEqual(ans2["4X 55"], 193);
        }

    }
}
