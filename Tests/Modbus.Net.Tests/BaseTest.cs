using Modbus.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class BaseTest
    {
        private List<AddressUnit> _addressUnits;

        [TestInitialize]
        public void Init()
        {
            _addressUnits = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Area = "3X",
                    Address = 1,
                    SubAddress = 0,
                    DataType = typeof(bool)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 1,
                    SubAddress = 1,
                    DataType = typeof(bool)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 1,
                    SubAddress = 2,
                    DataType = typeof(bool)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 2,
                    SubAddress = 0,
                    DataType = typeof(byte)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 2,
                    SubAddress = 8,
                    DataType = typeof(byte)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 3,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 4,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 6,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 9,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 10,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Area = "3X",
                    Address = 100,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Area = "4X",
                    Address = 1,
                    SubAddress = 0,
                    DataType = typeof(uint)
                },
                new AddressUnit
                {
                    Area = "4X",
                    Address = 4,
                    SubAddress = 0,
                    DataType = typeof(ushort)
                },
            };
        }

        [TestMethod]
        public void AddressCombinerContinusTest()
        {
            var addressCombiner = new AddressCombinerContinus(new AddressTranslatorModbus());
            var combinedAddresses = addressCombiner.Combine(_addressUnits).ToArray();
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
        public void AddressCombinerSingleTest()
        {
            var addressCombiner = new AddressCombinerSingle();
            var combinedAddresses = addressCombiner.Combine(_addressUnits).ToArray();
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
            var addressCombiner = new AddressCombinerNumericJump(10, new AddressTranslatorModbus());
            var combinedAddresses = addressCombiner.Combine(_addressUnits).ToArray();
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
        public void AddressCombinerPercentageJumpTest()
        {
            var addressCombiner = new AddressCombinerPercentageJump(30.0, new AddressTranslatorModbus());
            var combinedAddresses = addressCombiner.Combine(_addressUnits).ToArray();
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
    }
}
