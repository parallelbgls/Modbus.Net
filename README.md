Modbus.Net Oveview
===================

Overview
-------------------
Modbus.Net is an open hardware communication platform written by C# 7.0.

You can only focusing on the protocal itself, and the platform can automaticaly create a full asynchronized or synchronized communication library.

Why called Modbus.Net
-------------------
Modbus.Net was opened two years ago when I graduated. The first target of this project is to implement a remote PLC communication with Modbus TCP. But things were going changed after half a year. When the company decide to use a IoT hardware, a universary architech should be required. Then the main platform changed to a universal communication platform. But the name "Modbus.Net" holded back.

The real Modbus Implementation has been moved to Modbus.Net.Modbus. If you want a real Modbus C# implementation, please download "Modbus.Net" and "Modbus.Net.Modbus" at the same time.

There are also "Modbus.Net.Siemens" that can communicate to Siemens S7-200, S7-200 Smart, S7-300, S7-400, S7-1200, S7-1500 using PPI or TCP/IP.

"Modbus.Net.OPC" Implements OPC DA and OPC UA protocal.

Platform Supported
-------------------
* Visual Studio 2017
* .Net Framework 4.5
* .Net Standard 1.5

##RoadMap

###Version 1.2.0
* Modbus ASCII Support (Complete)
* Siemens PPI Support (Complete)
* OPC Write Data (Complete)
* Get and set bit value (Complete)
* Unit test (Complete)
* New Document (Complete)       
* New Samples (Complete)

###Version 1.2.2
* Address Utility (Complete)
* More functions in TaskManager (Complete)
* More interfaces (Complete)

###Version 1.2.3
* Endian Problem Fix (Complete)
* Name mode in TaskManager (Complete)

###Version 1.2.4
* OPC UA Support (Complete)
* OPC Regex comparer for tags (Complete)

###Version 1.3.0
* .NET Core Support (Not complete)

###Version 1.4.0
* New Protocal Pipeline System (In Road)
* New ComConnector (In Road)
* Multi station Modbus RTU, ASCII and Siemens PPI (In Road)
* Siemens MPI Support (In Road)
* Github wiki Document (In Road)
