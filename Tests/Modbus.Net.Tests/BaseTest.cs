using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;
using Modbus.Net.Siemens;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class BaseTest
    {
        private List<AddressUnit<int>>? _addressUnits;

        private BaseMachine<int, int>? _baseMachine2;

        [TestInitialize]
        public void Init()
        {
            _addressUnits = new List<AddressUnit<int>>
            {
                new AddressUnit<int>
                {
                    Id = 1,
                    Area = "3X",
                    Address = 1,
                    SubAddress = 0,
                    DataType = typeof(bool)
                },
                new AddressUnit<int>
                {
                    Id = 2,
                    Area = "3X",
                    Address = 1,
                    SubAddress = 1,
                    DataType = typeof(bool)
                },
                new AddressUnit<int>
                {
                    Id = 3,
                    Area = "3X",
                    Address = 1,
                    SubAddress = 2,
                    DataType = typeof(bool)
                },
                new AddressUnit<int>
                {
                    Id = 4,
                    Area = "3X",
                    Address = 2,
                    SubAddress = 0,
                    DataType = typeof(byte)
                },
                new AddressUnit<int>
                {
                    Id = 5,
                    Area = "3X",
                    Address = 2,
                    SubAddress = 8,
                    DataType = typeof(byte)
                },
                new AddressUnit<int>
                {
                    Id = 6,
                    Area = "3X",
                    Address = 3,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 7,
                    Area = "3X",
                    Address = 4,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 8,
                    Area = "3X",
                    Address = 6,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 9,
                    Area = "3X",
                    Address = 9,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 10,
                    Area = "3X",
                    Address = 10,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 11,
                    Area = "3X",
                    Address = 100,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit<int>
                {
                    Id = 12,
                    Area = "4X",
                    Address = 1,
                    SubAddress = 0,
                    DataType = typeof(uint)
                },
                new AddressUnit<int>
                {
                    Id = 13,
                    Area = "4X",
                    Address = 4,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
            };

            _baseMachine2 = new SiemensMachine<int, int>(2, SiemensType.Tcp, "192.168.3.10", SiemensMachineModel.S7_1200, _addressUnits, true, 2, 0)
            {
                ProjectName = "Project 1",
                MachineName = "Test 2"
            };

            _baseMachine2.ConnectAsync().Wait();
        }

        [TestMethod]
        public void AddressCombinerContinusTest()
        {
            var addressCombiner = new AddressCombinerContinus<int>(new AddressTranslatorModbus(), 100000);
            var combinedAddresses = addressCombiner.Combine(_addressUnits!).ToArray();
            Assert.AreEqual(combinedAddresses[0].Area, "3X");
            Assert.AreEqual(combinedAddresses[0].Address, 1);
            Assert.AreEqual(combinedAddresses[0].GetCount, 1);
            Assert.AreEqual(combinedAddresses[1].Area, "3X");
            Assert.AreEqual(combinedAddresses[1].Address, 2);
            Assert.AreEqual(combinedAddresses[1].GetCount, 6);
            Assert.AreEqual(combinedAddresses[2].Area, "3X");
            Assert.AreEqual(combinedAddresses[2].Address, 6);
            Assert.AreEqual(combinedAddresses[2].GetCount, 2);
            Assert.AreEqual(combinedAddresses[3].Area, "3X");
            Assert.AreEqual(combinedAddresses[3].Address, 9);
            Assert.AreEqual(combinedAddresses[3].GetCount, 4);
            Assert.AreEqual(combinedAddresses[4].Area, "3X");
            Assert.AreEqual(combinedAddresses[4].Address, 100);
            Assert.AreEqual(combinedAddresses[4].GetCount, 2);
            Assert.AreEqual(combinedAddresses[5].Area, "4X");
            Assert.AreEqual(combinedAddresses[5].Address, 1);
            Assert.AreEqual(combinedAddresses[5].GetCount, 4);
            Assert.AreEqual(combinedAddresses[6].Area, "4X");
            Assert.AreEqual(combinedAddresses[6].Address, 4);
            Assert.AreEqual(combinedAddresses[6].GetCount, 2);
        }

        [TestMethod]
        public void AddressCombinerContinusLimitTest()
        {
            var addressCombiner = new AddressCombinerContinus<int>(new AddressTranslatorModbus(), 4);
            var combinedAddresses = addressCombiner.Combine(_addressUnits!).ToArray();
            Assert.AreEqual(combinedAddresses[0].Area, "3X");
            Assert.AreEqual(combinedAddresses[0].Address, 1);
            Assert.AreEqual(combinedAddresses[0].GetCount, 1);
            Assert.AreEqual(combinedAddresses[1].Area, "3X");
            Assert.AreEqual(combinedAddresses[1].Address, 2);
            Assert.AreEqual(combinedAddresses[1].GetCount, 4);
            Assert.AreEqual(combinedAddresses[2].Area, "3X");
            Assert.AreEqual(combinedAddresses[2].Address, 2);
            Assert.AreEqual(combinedAddresses[2].GetCount, 2);
            Assert.AreEqual(combinedAddresses[3].Area, "3X");
            Assert.AreEqual(combinedAddresses[3].Address, 6);
            Assert.AreEqual(combinedAddresses[3].GetCount, 2);
            Assert.AreEqual(combinedAddresses[4].Area, "3X");
            Assert.AreEqual(combinedAddresses[4].Address, 9);
            Assert.AreEqual(combinedAddresses[4].GetCount, 4);
            Assert.AreEqual(combinedAddresses[5].Area, "3X");
            Assert.AreEqual(combinedAddresses[5].Address, 100);
            Assert.AreEqual(combinedAddresses[5].GetCount, 2);
            Assert.AreEqual(combinedAddresses[6].Area, "4X");
            Assert.AreEqual(combinedAddresses[6].Address, 1);
            Assert.AreEqual(combinedAddresses[6].GetCount, 4);
            Assert.AreEqual(combinedAddresses[7].Area, "4X");
            Assert.AreEqual(combinedAddresses[7].Address, 4);
            Assert.AreEqual(combinedAddresses[7].GetCount, 2);
        }

        [TestMethod]
        public void AddressCombinerSingleTest()
        {
            var addressCombiner = new AddressCombinerSingle<int>();
            var combinedAddresses = addressCombiner.Combine(_addressUnits!).ToArray();
            Assert.AreEqual(combinedAddresses[0].Area, "3X");
            Assert.AreEqual(combinedAddresses[0].Address, 1);
            Assert.AreEqual(combinedAddresses[0].GetCount, 1);
            Assert.AreEqual(combinedAddresses[1].Area, "3X");
            Assert.AreEqual(combinedAddresses[1].Address, 1);
            Assert.AreEqual(combinedAddresses[1].SubAddress, 1);
            Assert.AreEqual(combinedAddresses[1].GetCount, 1);
            Assert.AreEqual(combinedAddresses[3].Area, "3X");
            Assert.AreEqual(combinedAddresses[3].Address, 2);
            Assert.AreEqual(combinedAddresses[3].GetCount, 1);
            Assert.AreEqual(combinedAddresses[4].Area, "3X");
            Assert.AreEqual(combinedAddresses[4].Address, 2);
            Assert.AreEqual(combinedAddresses[4].SubAddress, 8);
            Assert.AreEqual(combinedAddresses[4].GetCount, 1);
            Assert.AreEqual(combinedAddresses[11].Area, "4X");
            Assert.AreEqual(combinedAddresses[11].Address, 1);
            Assert.AreEqual(combinedAddresses[11].GetCount, 1);
        }

        [TestMethod]
        public void AddressCombinerNumericJumpTest()
        {
            var addressCombiner = new AddressCombinerNumericJump<int>(10, 100000, new AddressTranslatorModbus());
            var combinedAddresses = addressCombiner.Combine(_addressUnits!).ToArray();
            Assert.AreEqual(combinedAddresses[0].Area, "3X");
            Assert.AreEqual(combinedAddresses[0].Address, 1);
            Assert.AreEqual(combinedAddresses[0].GetCount, 20);
            Assert.AreEqual(combinedAddresses[1].Area, "3X");
            Assert.AreEqual(combinedAddresses[1].Address, 100);
            Assert.AreEqual(combinedAddresses[1].GetCount, 2);
            Assert.AreEqual(combinedAddresses[2].Area, "4X");
            Assert.AreEqual(combinedAddresses[2].Address, 1);
            Assert.AreEqual(combinedAddresses[2].GetCount, 8);
        }

        [TestMethod]
        public void AddressCombinerNumericJumpLimitTest()
        {
            var addressCombiner = new AddressCombinerNumericJump<int>(10, 10, new AddressTranslatorModbus());
            var combinedAddresses = addressCombiner.Combine(_addressUnits!).ToArray();
            Assert.AreEqual(combinedAddresses[0].Area, "3X");
            Assert.AreEqual(combinedAddresses[0].Address, 1);
            Assert.AreEqual(combinedAddresses[0].GetCount, 8);
            Assert.AreEqual(combinedAddresses[1].Area, "3X");
            Assert.AreEqual(combinedAddresses[1].Address, 6);
            Assert.AreEqual(combinedAddresses[1].GetCount, 10);
            Assert.AreEqual(combinedAddresses[2].Area, "3X");
            Assert.AreEqual(combinedAddresses[2].Address, 100);
            Assert.AreEqual(combinedAddresses[2].GetCount, 2);
            Assert.AreEqual(combinedAddresses[3].Area, "4X");
            Assert.AreEqual(combinedAddresses[3].Address, 1);
            Assert.AreEqual(combinedAddresses[3].GetCount, 8);
        }

        [TestMethod]
        public void AddressCombinerPercentageJumpTest()
        {
            var addressCombiner = new AddressCombinerPercentageJump<int>(30.0, 100000, new AddressTranslatorModbus());
            var combinedAddresses = addressCombiner.Combine(_addressUnits!).ToArray();
            Assert.AreEqual(combinedAddresses[0].Area, "3X");
            Assert.AreEqual(combinedAddresses[0].Address, 1);
            Assert.AreEqual(combinedAddresses[0].GetCount, 12);
            Assert.AreEqual(combinedAddresses[1].Area, "3X");
            Assert.AreEqual(combinedAddresses[1].Address, 9);
            Assert.AreEqual(combinedAddresses[1].GetCount, 4);
            Assert.AreEqual(combinedAddresses[2].Area, "3X");
            Assert.AreEqual(combinedAddresses[2].Address, 100);
            Assert.AreEqual(combinedAddresses[2].GetCount, 2);
            Assert.AreEqual(combinedAddresses[3].Area, "4X");
            Assert.AreEqual(combinedAddresses[3].Address, 1);
            Assert.AreEqual(combinedAddresses[3].GetCount, 8);
        }

        [TestMethod]
        public void AddressCombinerPercentageJumpLimitTest()
        {
            var addressCombiner = new AddressCombinerPercentageJump<int>(30.0, 10, new AddressTranslatorModbus());
            var combinedAddresses = addressCombiner.Combine(_addressUnits!).ToArray();
            Assert.AreEqual(combinedAddresses[0].Area, "3X");
            Assert.AreEqual(combinedAddresses[0].Address, 1);
            Assert.AreEqual(combinedAddresses[0].GetCount, 8);
            Assert.AreEqual(combinedAddresses[1].Area, "3X");
            Assert.AreEqual(combinedAddresses[1].Address, 6);
            Assert.AreEqual(combinedAddresses[1].GetCount, 2);
            Assert.AreEqual(combinedAddresses[2].Area, "3X");
            Assert.AreEqual(combinedAddresses[2].Address, 9);
            Assert.AreEqual(combinedAddresses[2].GetCount, 4);
            Assert.AreEqual(combinedAddresses[3].Area, "3X");
            Assert.AreEqual(combinedAddresses[3].Address, 100);
            Assert.AreEqual(combinedAddresses[3].GetCount, 2);
            Assert.AreEqual(combinedAddresses[4].Area, "4X");
            Assert.AreEqual(combinedAddresses[4].Address, 1);
            Assert.AreEqual(combinedAddresses[4].GetCount, 8);
        }

        [TestCleanup]
        public void MachineClean()
        {
            _baseMachine2?.Disconnect();
        }
    }
}
