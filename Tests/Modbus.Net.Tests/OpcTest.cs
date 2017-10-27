using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.OPC;
using Modbus.Net.OPC.FBox;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class OpcTest
    {
        private BaseMachine _opcMachine;

        [TestInitialize]
        public void Init()
        {
            _opcMachine = new FBoxOpcDaMachine("1", "1", "德联科技测试", null, true);
        }

        [TestMethod]
        public async Task OpcSingle()
        {
            _opcMachine.GetAddresses = new List<AddressUnit>
            {
                new AddressUnit()
                {
                    Id = "1",
                    Name = "蒸汽压力下限",
                    Area = "0",
                    Address = 1,
                    DataType = typeof(ushort)
                }
            };
            var ans = await _opcMachine.GetDatasAsync(MachineGetDataType.Id);
            Assert.AreEqual(ans["1"].PlcValue, 525);
        }

        [TestMethod]
        public async Task OpcMultiple()
        {
            _opcMachine.GetAddresses = new List<AddressUnit>
            {
                new AddressUnit()
                {
                    Id = "1",
                    Name = "蒸汽压力下限",
                    Area = "0",
                    Address = 1,
                    DataType = typeof(ushort)
                },
                new AddressUnit()
                {
                    Id = "2",
                    Name = "蒸汽压力目标",
                    Area = "0",
                    Address = 2,
                    DataType = typeof(ushort)
                },
                new AddressUnit()
                {
                    Id = "3",
                    Name = "蒸汽压力上限",
                    Area = "0",
                    Address = 3,
                    DataType = typeof(ushort)
                }
            };
            var ans = await _opcMachine.GetDatasAsync(MachineGetDataType.Id);
            Assert.AreEqual(ans["1"].PlcValue, 525);
            Assert.AreEqual(ans["2"].PlcValue, 600);
            Assert.AreEqual(ans["3"].PlcValue, 650);
        }

        [TestMethod]
        public async Task OpcWrite()
        {
            _opcMachine.GetAddresses = new List<AddressUnit>
            {
                new AddressUnit()
                {
                    Id = "1",
                    Name = "蒸汽压力下限",
                    Area = "0",
                    Address = 1,
                    DataType = typeof(ushort)
                }
            };
            var success = await _opcMachine.SetDatasAsync(MachineSetDataType.Id, new Dictionary<string, double>
            {
                {
                    "1", 525
                }
            });
            Assert.AreEqual(success, true);
            var ans = await _opcMachine.GetDatasAsync(MachineGetDataType.Id);
            Assert.AreEqual(ans["1"].PlcValue, 525);
        }

        [TestCleanup]
        public void MachineClean()
        {
            _opcMachine.Disconnect();
        }
    }
}
