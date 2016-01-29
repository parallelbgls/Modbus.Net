Modbus.Net
===================

An automation communication Library written by C#.

Caution: I really want to implement the COM communication system, but nowaday Usb to Serial cable is really hard to be driven in Win8.1 and Win10. So finally, I'm really sorry to tell that the maintainence of ComConnector has been stopped. Although Modbus RTU will remain in this project, maintanence will also be stopped. And there are no future support for Modbus ASCII, Siemens PPI and Siemens MPI. 

Caution2: In the current version, you can't get bit or set bit in this library. Please get a byte, change the bit value in the byte, and set to PLC. I will fix this bug in future.

Reference: <b>"h-opc"</b> is linked by [https://github.com/hylasoft-usa/h-opc](https://github.com/hylasoft-usa/h-opc)

Table of Content:
* [Features](#features)
* [Usage](#usage)
* [Architecture](#architecture)
* [Quick Start](#quick_start)

##<a name="features"></a> Features
* A open platform that you can easy to extend a industrial communication protocal.
* Modbus Tcp protocal.
* Siemens Tcp protocal (acturally it is the same as Profinet)
* OPC DA protocal.
* All communications can be asyncronized.
* A task manager that you can easily manage multiple connections.
* .net framework 4.5 and Visual Studio 2015 support.

##<a name="usage"></a> Usage

###Samples:

There are four samples in the project. All sample project recommand running in Siemens 200 PLC.

* NA200H.UI.ConsoleApp -- The simplest and lowest api test, they are all commented and you can uncomment any part to see the sample.
* NA200H.UI.WPF -- A three number add sample project.
* CrossLampControl.Webapi -- A web api project that show and control a cross lamp sample code in Siemens 200 PLC(PLC code is also in the project).
* Siemens_S7_200.UI.WPF.TaskTest -- A sample code using Task Manager.

##<a name="architecture"></a> Architecture

###Connector

Connector implements the basic connecting method, like Socket and SignalR.

###ProtocalLinker

ProtocalLinker implements the link, send, and send recevice actions.

###ProtocalLinkerBytesExtend

Some Protocal has the same head or tail in the same connection way, but different in different connection way, so bytes extend can extend sending message when they are sended in different type of connection.

###ProtocalUnit

Format and deformat the protocal message between the structual class and bytes array.

###ValueHelper

Help change the value between number and bytes, or number array to bytes array.

###Protocal

Manage all protocals and implement a lazy loading method.

###Utility

Manage several types of Protocal to a same calling interface.

###Machine

Shows the Hardware PLC or other types of machine and implement a high level send and receive api.

###TaskManager

The highest api that you can manage many PLC links and all links are async so there are no block in all connections.

##<a name="quick_start"></a> Quick Start.

The fastest way you can write is to use TaskManager. TaskTest Project is a good example.


```C#
List<AddressUnit> addressUnits = new List<AddressUnit>
{
    new AddressUnit() {Id = 0, Area = "V", Address = 0, CommunicationTag = "D1", DataType = typeof (ushort), Zoom = 1},
    new AddressUnit() {Id = 1, Area = "V", Address = 2, CommunicationTag = "D2", DataType = typeof (float), Zoom = 1}
};
TaskManager task = new TaskManager(300, true);
task.AddMachine(new SiemensMachine(SiemensType.Tcp, "192.168.3.191",SiemensMachineModel.S7_200, addressUnits, true));
task.ReturnValues += (returnValues) =>
{
    value = new List<string>();
    if (returnValues.Value != null)
    {
        value = from val in returnValues.Value select val.Key + val.Value;
        siemensItems.Dispatcher.Invoke(() => siemensItems.ItemsSource = value);
    }
    else
    {
        Console.WriteLine(String.Format("ip {0} not return value", returnValues.Key));
    }
};
task.TaskStart();
```


Here are the details to use the TaskManager.

1. Initialize the task manager. 
Three arguments are: <s>the max tasks you can run in a same time;</s> How long did the next send message call happens(milliseconds); and you should keep the connection when a single message called complete.

2. Add the addresses that you want to communicate to PLC. Area are defined in AddressTranslator in each type of communiction.
Basically you can write only Id, Area, Address, CommunicationTag, DataType and Zoom, and it should work. And there are other fields that you can use. 
More important, you can extend and implement your own field in UnitExtend in every AddressUnit, and it will return in return event.

3. Add a machine to TaskManager.
Add a machine like siemens machine to the task manager.

4. Implement ReturnValues event.
The argument return values is a key value pair. The architechture is:

  * Key : the link address of machine (in sample is the second parameter).<p>
  * Value : Dictionary.<p>
    * Key : CommunicationTag in AddressUnit.<p>
    * Value : ReturnUnit.<p>
      * PlcValue : The return data, all in double type.<p>
      * UnitExtend : UnitExtend in AddressUnit. You should cast this class to your own class extends by UnitExtend.