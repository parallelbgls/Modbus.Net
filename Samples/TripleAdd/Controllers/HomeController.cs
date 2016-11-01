using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Modbus.Net;
using Modbus.Net.Modbus;
using TripleAdd.Models;

namespace TripleAdd.Controllers
{
    public class HomeController : Controller
    {
        private static BaseUtility utility;
        private static BaseMachine machine;

        public ActionResult Index()
        {
            return RedirectToAction("Machine");
        }

        public async Task<ActionResult> Machine()
        {
            var data = await GetMachineEnter();
            ViewBag.Method = "SetMachine";
            return View("Index", data);
        }

        public async Task<ActionResult> Utility()
        {
            var data = await GetUtilityEnter();
            ViewBag.Method = "SetUtility";
            return View("Index", data);
        }

        private async Task<TripleAddViewModel> GetUtilityEnter()
        {
            if (utility == null)
            {
                utility = new ModbusUtility(ModbusType.Tcp, "192.168.3.10", 2, 0);
                utility.AddressTranslator = new AddressTranslatorNA200H();
            }
            object[] getNum = await utility.GetDatasAsync("4X 1", new KeyValuePair<Type, int>(typeof(ushort), 4));
            ushort[] getNumUshorts = BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<ushort>(getNum);
            return SetValue(getNumUshorts);
        }

        private async Task<TripleAddViewModel> GetMachineEnter()
        {
            if (machine == null)
            {
                machine = new ModbusMachine(ModbusType.Tcp, "192.168.3.10", new List<AddressUnit>()
                {
                    new AddressUnit() {Id = "1", Area = "4X", Address = 1, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    new AddressUnit() {Id = "2", Area = "4X", Address = 2, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    new AddressUnit() {Id = "3", Area = "4X", Address = 3, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    new AddressUnit() {Id = "4", Area = "4X", Address = 4, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                }, 2, 0);
                machine.AddressCombiner = new AddressCombinerContinus(machine.AddressTranslator);
                machine.AddressCombinerSet = new AddressCombinerContinus(machine.AddressTranslator);
            }
            var resultFormat = (await machine.GetDatasAsync(MachineGetDataType.CommunicationTag)).MapGetValuesToSetValues();
            return SetValue(new ushort[4] { (ushort)resultFormat["Add1"], (ushort)resultFormat["Add2"], (ushort)resultFormat["Add3"], (ushort)resultFormat["Ans"] });
        }

        private TripleAddViewModel SetValue(ushort[] getNum)
        {
            return new TripleAddViewModel()
            {
                Add1 = getNum[0],
                Add2 = getNum[1],
                Add3 = getNum[2],
                Ans = getNum[3],
            };
        }

        [HttpPost]
        public async Task<JsonResult> SetUtility(TripleAddViewModel model)
        {
            ushort add1 = model.Add1, add2 = model.Add2, add3 = model.Add3;
            var ans = await await utility.SetDatasAsync("4X 1", new object[] { add1, add2, add3 }).ContinueWith(async p => await GetUtilityEnter());
            return Json(ans, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> SetMachine(TripleAddViewModel model)
        {
            ushort add1 = model.Add1, add2 = model.Add2, add3 = model.Add3;
            var setDic = new Dictionary<string, double> { { "Add1", add1 }, { "Add2", add2 }, { "Add3", add3 } };
            var ans = await await machine.SetDatasAsync(MachineSetDataType.CommunicationTag, setDic).ContinueWith(async p =>await GetMachineEnter());
            return Json(ans, JsonRequestBehavior.AllowGet);
        }
    }
}