using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class MachineUtilityMethodTest
    {
        [TestMethod]
        public void GetUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(ModbusType.Tcp, "192.168.3.12", null, true, 2, 0);
            var utility = baseMachine.GetUtility<IUtilityTime>();
            var methods = utility.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Assert.AreEqual(methods.FirstOrDefault(method => method.Name == "GetTimeAsync") != null, true);
            Assert.AreEqual(methods.FirstOrDefault(method => method.Name == "SetTimeAsync") != null, true);
        }

        [TestMethod]
        public async Task InvokeUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(ModbusType.Tcp, "192.168.3.12", null, true, 2, 0);
            var success = baseMachine.InvokeUtilityMethod<IUtilityTime, Task<bool>>("SetTimeAsync", DateTime.Now);
            Assert.AreEqual(await success, true);
            var time = baseMachine.InvokeUtilityMethod<IUtilityTime, Task<DateTime>>("GetTimeAsync");
            Assert.AreEqual(((await time).ToUniversalTime() - DateTime.Now.ToUniversalTime()).Seconds < 10, true);
        }

        [TestMethod]
        public async Task InvokeMachine()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(ModbusType.Tcp, "192.168.3.12", new List<AddressUnit<int>>
            {
                new AddressUnit<int>
                {
                    Id = 0,
                    Area = "0X",
                    Address = 1,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(bool)
                }
            }, true, 2, 0);
            var success = baseMachine.InvokeMachineMethod<IMachineData, Task<bool>>("SetDatasAsync",
                MachineSetDataType.Address,
                new Dictionary<string, double>
                {
                    {
                        "0X 1.0", 1
                    }
                });
            Assert.AreEqual(await success, true);
            var datas = baseMachine.InvokeMachineMethod<IMachineData, Task<Dictionary<string, ReturnUnit>>>("GetDatasAsync", MachineGetDataType.Address);
            Assert.AreEqual((await datas)["0X 1.0"], 1);
        }
    }
}
