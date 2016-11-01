using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CrossLampControl.WebApi.Models;
using Modbus.Net;
using Modbus.Net.Modbus;

namespace CrossLamp.Controllers
{
    public class HomeController : Controller
    {
        private static BaseUtility _utility;

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetLamp()
        {
            try
            {
                if (_utility == null)
                {
                    _utility = new ModbusUtility(ModbusType.Tcp, "192.168.3.10", 2, 0);
                }
                Lamp light = new Lamp();
                object[] lampsbyte = await _utility.GetDatasAsync("0X 1", new KeyValuePair<Type, int>(typeof (bool), 7));
                bool[] lamps = BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<bool>(lampsbyte);
                if (lamps[0])
                {
                    light.MainLamp = LightLamp.Red.ToString();
                }
                else if (lamps[1])
                {
                    light.MainLamp = LightLamp.Yellow.ToString();
                }
                else
                {
                    light.MainLamp = LightLamp.Green.ToString();
                }
                if (lamps[3])
                {
                    light.SubLamp = LightLamp.Red.ToString();
                }
                else if (lamps[4])
                {
                    light.SubLamp = LightLamp.Yellow.ToString();
                }
                else
                {
                    light.SubLamp = LightLamp.Green.ToString();
                }
                light.SetStart(lamps[6]);
                return Json(light, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}