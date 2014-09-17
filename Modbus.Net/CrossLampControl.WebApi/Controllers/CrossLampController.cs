using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using CrossLampControl.WebApi.Models;
using ModBus.Net;

namespace CrossLampControl.WebApi.Controllers
{
    public class CrossLampController : ApiController
    {
        ModbusUtility _utility = new ModbusUtility("COM6", (int)ModbusType.Rtu);
        [HttpGet]
        public Lamp GetLamp()
        {
            Lamp light = new Lamp();
            bool[] lamps = _utility.GetCoils(2, "0", 6);
            if (lamps[0])
            {
                light.MainLamp = LightLamp.Green.ToString();
            }
            else if (lamps[1])
            {
                light.MainLamp = LightLamp.Yellow.ToString();
            }
            else
            {
                light.MainLamp = LightLamp.Red.ToString();
            }
            if (lamps[3])
            {
                light.SubLamp = LightLamp.Green.ToString();
            }
            else if (lamps[4])
            {
                light.SubLamp = LightLamp.Yellow.ToString();
            }
            else
            {
                light.SubLamp = LightLamp.Red.ToString();
            }
            light.SetStart(lamps[6]);
            return light;
        }

    }
}
