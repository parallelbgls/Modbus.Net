Modbus.Net Overview
===================

Overview
-------------------
Modbus.Net is an open hardware communication platform.

You can focus on the protocol itself and the platform can automatically create a full asynchronous or synchronous communication library.

Important
-------------------
You need to copy appsettings.default.json file in Modbus.Net to your own project, and don't forget to change settings of file to copy content and copy when newer, otherwise Modbus.Net will not work.

Why is it called Modbus.Net
-------------------
Modbus.Net was open sourced in 2019 when I graduated. The first target of this project was to implement remote PLC communication using Modbus TCP. Half a year later the company decide to use a IoT hardware, then a more universal architecture was required. The main platform changed to a universal communication platform. Despite all these changes the name "Modbus.Net" stuck.

The real Modbus Implementation has been moved to [Modbus.Net.Modbus]( https://www.nuget.org/packages/Modbus.Net.Modbus). If you want a real Modbus C# implementation, please download [Modbus.Net]( https://www.nuget.org/packages/Modbus.Net) and [Modbus.Net.Modbus]( https://www.nuget.org/packages/Modbus.Net.Modbus) at the same time.

There is also [Modbus.Net.Siemens]( https://www.nuget.org/packages/Modbus.Net.Siemens) that can communicate with Siemens S7-200, S7-200 Smart, S7-300, S7-400, S7-1200 and S7-1500 using PPI or TCP/IP.

[Modbus.Net.Opc]( https://www.nuget.org/packages/Modbus.Net.Opc) Implements OPC DA and OPC UA protocol.

Supported Platforms
-------------------
* Visual Studio 2022
* .NET 6.0

Thanks
-------------------
* Quartz - Job Scheduler
* Serilog - Logging
* DotNetty - Network Transporting
