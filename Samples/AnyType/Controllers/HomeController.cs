/** Use Automation/Automation/TaskManager Project
 * 使用 Automation/Automation/TaskManager 工程
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AnyType.Models;
using Modbus.Net;
using Modbus.Net.Modbus;

namespace AnyType.Controllers
{
    public class HomeController : Controller
    {
        private static IEnumerable<TaskViewModel> values = new List<TaskViewModel>();
        private static TaskManager task;

        public ActionResult Index()
        {
            if (task == null)
            {
                StartTask();
            }
            return View(values.ToList());
        }

        private void StartTask()
        {

            //增加需要通信的PLC地址
            List<AddressUnit> addressUnits = new List<AddressUnit>
            {
                new AddressUnit() {Id = "d0", Name="Variable 1", Area = "4X", Address = 1, CommunicationTag = "D1", DataType = typeof (ushort), Zoom = 0.01, DecimalPos = 2},
                new AddressUnit() {Id = "d1", Name="Variable 2", Area = "4X", Address = 2, SubAddress = 0, CommunicationTag = "D2", DataType = typeof (bool)},
                new AddressUnit() {Id = "d2", Name="Variable 3", Area = "4X", Address = 2, SubAddress = 1, CommunicationTag = "D3", DataType = typeof (bool)},
                new AddressUnit() {Id = "d3", Name="Variable 4", Area = "4X", Address = 2, SubAddress = 8, CommunicationTag = "D4", DataType = typeof (byte)},
                new AddressUnit() {Id = "d4", Name="Variable 5", Area = "4X", Address = 3, CommunicationTag = "D5", DataType = typeof (int)},
                new AddressUnit() {Id = "d5", Name="Variable 6", Area = "4X", Address = 5, CommunicationTag = "D6", Zoom=0.1, DecimalPos = 1, DataType = typeof (float)},
                new AddressUnit() {Id = "d6", Name="Variable 7", Area = "4X", Address = 7, CommunicationTag = "D7", DecimalPos = 1, DataType = typeof (float)},
                new AddressUnit() {Id = "d7", Name="Variable 8", Area = "4X", Address = 9, CommunicationTag = "D8", DataType = typeof (short)},
            };

            values = from unitValue in addressUnits
                     select
                         new TaskViewModel()
                         {
                             Id = unitValue.Id,
                             Name = unitValue.Name,
                             Address = unitValue.Address + "." + unitValue.SubAddress,
                             Value = 0,
                             Type = unitValue.DataType.Name
                         };

            //初始化任务管理器
            task = new TaskManager(10, true);
            //向任务管理器中添加设备
            task.AddMachine(new ModbusMachine(ModbusType.Tcp, "192.168.3.10", addressUnits,
            true, 2, 0));
            //启动任务
            task.InvokeTimerAll(new TaskItemGetData(returnValues =>
            {
                //唯一的参数包含返回值，是一个唯一标识符（machine的第二个参数），返回值（类型ReturnUnit）的键值对。
                if (returnValues.ReturnValues != null)
                {
                    lock (values)
                    {
                        var unitValues = from val in returnValues.ReturnValues
                            select
                            new Tuple<AddressUnit, double?>(
                                addressUnits.FirstOrDefault(p => p.CommunicationTag == val.Key), val.Value.PlcValue);
                        values = from unitValue in unitValues
                            select
                            new TaskViewModel()
                            {
                                Id = unitValue.Item1.Id,
                                Name = unitValue.Item1.Name,
                                Address = unitValue.Item1.Address + "." + unitValue.Item1.SubAddress,
                                Value = unitValue.Item2 ?? 0,
                                Type = unitValue.Item1.DataType.Name
                            };
                    }
                }
                else
                {
                    Console.WriteLine($"ip {returnValues.MachineId} not return value");
                }
            }, 15000, 60000));
        }

        [HttpGet]
        public JsonResult Get()
        {
            List<TaskViewModel> ans;
            lock (values)
            {
                ans = values.ToList();
            }
            return Json(ans, JsonRequestBehavior.AllowGet);
        }
    }
}