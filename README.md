Modbus.Net Overview
===================

* [Teambition Project](https://www.teambition.com/project/573860b0f668c69e61d38a84/tasks)

Overview
-------------------
Modbus.Net is an open hardware communication platform written in C# 7.0.

You can focus on the protocol itself and the platform can automatically create a full asynchronous or synchronous communication library.

Why is it called Modbus.Net
-------------------
Modbus.Net was open sourced two years ago when I graduated. The first target of this project was to implement remote PLC communication using Modbus TCP. Half a year later the company decide to use a IoT hardware, then a more universal architecture was required. The main platform changed to a universal communication platform. Despite all these changes the name "Modbus.Net" stuck.

The real Modbus Implementation has been moved to [Modbus.Net.Modbus]( https://www.nuget.org/packages/Modbus.Net.Modbus). If you want a real Modbus C# implementation, please download [Modbus.Net]( https://www.nuget.org/packages/Modbus.Net) and [Modbus.Net.Modbus]( https://www.nuget.org/packages/Modbus.Net.Modbus) at the same time.

There is also [Modbus.Net.Siemens]( https://www.nuget.org/packages/Modbus.Net.Siemens) that can communicate with Siemens S7-200, S7-200 Smart, S7-300, S7-400, S7-1200 and S7-1500 using PPI or TCP/IP.

[Modbus.Net.OPC]( https://www.nuget.org/packages/Modbus.Net.OPC) Implements OPC DA and OPC UA protocol.

Supported Platforms
-------------------
* Visual Studio 2017
* .NET Framework 4.5
* .NET Standard 2.0

Thanks
-------------------
Resharper -- Offers Modbus.Net team community license.

RoadMap
-------------------

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
* New log system using Serilog (Complete)

### Version 1.3.6
* Add gereric Type for BaseConnector, now protocol developer can pass any type to BaseConnector not only byte[] (Complete)
* Add more gereric types in Modbus.Net to support this function (Complete)
* Add more interfaces to make them completed in Modbus.Net (Complete)
* Support this function in Modbus.Net.OPC (Complete)

### Version 1.3.7
* AddressCombiner need to add maximum length now. Combiner will consider the maximum length when combining addresses (Complete)

### Version 1.3.8
* Change Resx to appsettings.json, now you can set default params there (Complete - CORE ONLY)
* Change ISpecialProtocalUnit to SpecialProtocalUnitAttribute (Complete)

### Version 1.3.9
* Modbus Single Write for Coil and Reg (05 and 06) (Complete)
* Fix OPC tag combine problem (Complete)

### 1.3.X Other
* Github wiki Document Chinese (Complete)
* Github wiki Document English (Complete)

### Version 1.3.10
* Update to .Net Standard 2.0 (Complete)

### Version 1.4.0
* New Protocol Pipeline System (Complete)

### Version 1.4.1
* BaseController (Complete)
* New ComConnector (Complete)
* New TcpConnector (Complete)
* New UdpConnector (Complete)
* Serial Port Connection with Multiple Master Station (Complete)

### Version 1.4.2
* Machine Builder (In Progress)
* Architecture rebuild (Almost complete)

### Version 1.5.X
* PPI Remake (In Progress)
* Siemens MPI Support (In Progress)
* Siemens MultiStation PPI Support (In Progress)
* Passive Connector and Controller (In Progress)

### Version 1.6.X
* English comment (In Progress)
* ValueHelper remake to interface, users can add their own value translate function (In Progress)
* New Zoom (In Progress)

### Version 2.0.0
* Rename to Transport.Net (In Progress)
