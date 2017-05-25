Modbus.Net Oveview
===================

* [Teambition Project](https://www.teambition.com/project/573860b0f668c69e61d38a84/tasks)
* QQ群 438411519 Modbus.Net交流群

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
* .NET Framework 4.5
* .NET Standard 1.3

## RoadMap

### Version 1.2.0
* Modbus ASCII Support (Complete)
* Siemens PPI Support (Complete)
* OPC Write Data (Complete)
* Get and set bit value (Complete)
* Unit test (Complete)
* New Document (Complete)       
* New Samples (Complete)

### Version 1.2.2
* Address Utility (Complete)
* More functions in TaskManager (Complete)
* More interfaces (Complete)

### Version 1.2.3
* Endian Problem Fix (Complete)
* Name mode in TaskManager (Complete)

### Version 1.2.4
* OPC UA Support (Complete)
* OPC Regex comparer for tags (Complete)

### Version 1.3.0
* .NET Core Support (Complete)
* Fix a bug in BaseMachine (Complete)

### Version 1.3.1
* InputStruct -> IInputStruct, OutputStruct -> IOutputStruct (Complete)
* Generic Method For ProtocalUnit (Complete)

### Version 1.3.2
* Add Interface IMachineMethod and IUtilityMethod. Utiltiy and Machine can extend function using interface (Complete)

### Version 1.3.3
* TaskManager Remake (Complete)

### Version 1.3.4
* A Serial Port now can connect to multiple machines using same protocol with different slave address (Complete)

### Version 1.3.5
* New log system using serilog (Complete)

### Version 1.3.6
* Add gereric Type for BaseConnector, now protocol developer can pass any type to BaseConnector not only byte[] (Complete)
* Add more gereric types in Modbus.Net to support this function (Complete)
* Add more interfaces to make them completed in Modbus.Net (Complete)
* Support this function in Modbus.Net.OPC (Complete)

### Version 1.3.7
* AddressCombiner need to add maximum length now. Combiner will consider the maximum length when combining addresses (Complete)

### Version 1.3.8
* Change Resx to app.config or appsettings.json, now you can set default params there (Complete)
* Change ISpecialProtocalUnit to SpecialProtocalUnitAttribute (Complete)

### Version 1.3.9
* Modbus Single Write for Coil and Reg (05 and 06) (In Road)

* Github wiki Document Chinese (Not Complete)
* Github wiki Document English (In Road)

### Version 1.4.0
* BaseController (In Road)
* New ComConnector (In Road)
* Serial Port Connection with Multiple Master Station (In Road)

### Version 1.4.1
* New Protocal Pipeline System (In Road)
* PPI Remake (In Road)

### Version 1.4.2
* Siemens MPI Support (In Road)
* Siemens MultiStation PPI Support (In Road)

