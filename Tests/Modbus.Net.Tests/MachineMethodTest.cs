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
    public class MachineMethodTest
    {
        [TestMethod]
        public void GetUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(ModbusType.Tcp, "192.168.3.12", null, true, 2, 0);
            var utility = baseMachine.GetUtility<IUtilityTime>();
            var methods = utility.GetType().GetRuntimeMethods();
            Assert.AreEqual(methods.FirstOrDefault(method => method.Name == "GetTimeAsync") != null, true);
            Assert.AreEqual(methods.FirstOrDefault(method => method.Name == "SetTimeAsync") != null, true);
        }

        [TestMethod]
        public async Task InvokeUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(ModbusType.Tcp, "192.168.3.12", null, true, 2, 0);
            var success = await baseMachine.InvokeUtilityMethod<IUtilityTime, Task<bool>>("SetTimeAsync", DateTime.Now);
            Assert.AreEqual(success, true);
            var time = await baseMachine.InvokeUtilityMethod<IUtilityTime, Task<DateTime>>("GetTimeAsync");
            Assert.AreEqual((time.ToUniversalTime() - DateTime.Now.ToUniversalTime()).Seconds < 10, true);
        }

        [TestMethod]
        public async Task InvokeMachine()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(ModbusType.Tcp, "192.168.3.10", new List<AddressUnit<int>>
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
            var success = await baseMachine.InvokeMachineMethod<IMachineData, Task<bool>>("SetDatasAsync",
                MachineSetDataType.Address,
                new Dictionary<string, double>
                {
                    {
                        "0X 1.0", 1
                    }
                });
            Assert.AreEqual(success, true);
            var datas = await baseMachine.InvokeMachineMethod<IMachineData, Task<Dictionary<string, ReturnUnit>>>("GetDatasAsync", MachineGetDataType.Address);
            Assert.AreEqual(datas["0X 1.0"].PlcValue, 1);
            success = await baseMachine.InvokeMachineMethod<IMachineData, Task<bool>>("SetDatasAsync",
                MachineSetDataType.Address,
                new Dictionary<string, double>
                {
                    {
                        "0X 1.0", 0
                    }
                });
            Assert.AreEqual(success, true);
        }
    }
}
