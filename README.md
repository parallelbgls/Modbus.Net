Modbus.Net Oveview
===================

Overview
-------------------
Modbus.Net is an open hardware communication platform written by C# 6.0.

You can only focusing on the protocal itself, and the platform can automaticaly create a full asynchronized or synchronized communication library.

Why called Modbus.Net
-------------------
Modbus.Net was opened two years ago when I graduated. The first target of this project is to implement a remote PLC communication with Modbus TCP. But things were going changed after half a year. When the company decide to use a IoT hardware, a universary architech should be required. Then the main platform changed to a universal communication platform. But the name "Modbus.Net" holded back.

The real Modbus Implementation has been moved to Modbus.Net.Modbus. If you want a real Modbus C# implementation, please download "Modbus.Net" and "Modbus.Net.Modbus" at the same time.

There are also "Modbus.Net.Siemens" that can communicate to Siemens S7-200, S7-200 Smart, S7-300, S7-400, S7-1200, S7-1500 using PPI or TCP/IP.

"Modbus.Net.OPC" Implements OPC DA protocal.

Platform Supported
-------------------
* Visual Studio 2015
* .Net Framework 4.5
