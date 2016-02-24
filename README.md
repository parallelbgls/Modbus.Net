Modbus.Net
===================

An hardware communication Library written by C#.

Caution: I really want to implement the COM communication system, but nowaday Usb to Serial cable is really hard to be driven in Win8.1 and Win10. So finally, I'm really sorry to tell that the maintainence of ComConnector has been stopped. Although Modbus RTU will remain in this project, maintanence will also be stopped. And there are no future support for Modbus ASCII, Siemens PPI and Siemens MPI.

Caution2: In the current version, you can't get bit or set bit in this library. Please get a byte, change the bit value in the byte, and set to PLC. I will fix this bug in future.

Reference: <b>"h-opc"</b> is linked by [https://github.com/hylasoft-usa/h-opc](https://github.com/hylasoft-usa/h-opc)

Table of Content:
* [Features](#features)
* [Usage](#usage)
* [Architecture](#architecture)
* [Quick Start](#quick_start)
* [Tutorial](#tutorial)
* [Implement](#implement)

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

* Key : the link address of machine (in sample is the second parameter).
* Value : Dictionary.
* Key : CommunicationTag in AddressUnit.
* Value : ReturnUnit.
* PlcValue : The return data, all in double type.
* UnitExtend : UnitExtend in AddressUnit. You should cast this class to your own class extends by UnitExtend.

##<a name="tutorial"></a> Tutorial
This platform has three level APIs that you could use: Low level API called "BaseUtility"; Middle level API called "BaseMachine"; High level API called "TaskManager".

###BaseUtility
BaseUtility is a low level api, in this level you can get or set data only by byte array or object array. Here is an example.

```C#
string ip = "192.168.0.10";
BaseUtility utility = new ModbusUtility(ModbusType.Tcp, ip);
utility.AddressTranslator = new AddressTranslatorNA200H();
object[] getNum = utility.GetDatas(0x02, 0x00, "NW 1", new KeyValuePair<Type, int>(typeof(ushort), 4));
utility.SetDatas(0x02, 0x00, "NW 1", new object[] { (ushort)1, (ushort)2, (ushort)3 });
```

BaseUtility is an abstract class. You can check all apis in BaseUtility.cs in Modbus.Net project.

To use BaseUtility, follow these steps.

1.New a BaseUtility instance, but remember BaseUtility is an abstract class, you should new class inherit from it.
```C#
BaseUtility utility = new ModbusUtility(ModbusType.Tcp, ip);
```

2.If you want to change the AddressTranslator, you can write your own AddressTranslator and change it using
```C#
utility.AddressTranslator = new AddressTranslatorNA200H();
```
To write your own AddressTranslator, inherit AddressTranslator from AddressTranslator.cs in Modbus.Net and implement this function.
```C#
public abstract KeyValuePair<int,int> AddressTranslate(string address, bool isRead);
```
Return value key is Area Code and value is Address.

3.Use GetData and SetData Api in BaseUtility, like
```C#
object[] getNum = utility.GetDatas(0x02, 0x00, "NW 1", new KeyValuePair<Type, int>(typeof(ushort), 4));

utility.SetDatas(0x02, 0x00, "NW 1", new object[] { (ushort)1, (ushort)2, (ushort)3 });
```
Remember force set type of numbers because GetData and SetData Apis are type sensitive.

You can also use async functions like
```C#
object[] getNum = await utility.GetDatasAsync(0x02, 0x00, "NW 1", new KeyValuePair<Type, int>(typeof(ushort), 4));
```

###BaseMachine
BaseMachine is a middle level api, in this level you could get and set datas in a managable data structure for a single machine.
To understand this class, you have to see the AddressUnit first.
```C#
public class AddressUnit
{
public int Id { get; set; }
public string Area { get; set; }
public int Address { get; set; }
public Type DataType { get; set; }
public double Zoom { get; set; }
public int DecimalPos { get; set; }
public string CommunicationTag { get; set; }
public string Name { get; set; }
public string Unit { get; set; }
public UnitExtend UnitExtend { get; set; }
}
```
For some reasons, AddressUnit has two keys: Id and CommunicationTag, one is integer and the other is string.

* Details
    * Area: Address Area description. For example the area code of modbus address "4X 1" is "4X".
    * Address: Address number description. For example the address of modbus address "4X 1" is "1".
    * DataType: The DataType of value.
    * Zoom : Scale the return value. For example if zoom is 0.1 then return value 150 will change to 15.
    * DecimalPos : Keep to the places after decimal. For example if DecimalPos is 2 then 150.353 will change to 150.35.
    * Name : Name of the Address.
    * Unit : Unit of the Address. For example "¡æ".
    * UnitExtend : If you want to get something else when value returns, extend the class and give it to here.

Then using BaseMachine like this.
```C#
BaseMachine machine = new ModbusMachine(ModbusType.Tcp, "192.168.3.12", new List<AddressUnit>()
{
new AddressUnit() {Id = 1, Area = "NW", Address = 1, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
new AddressUnit() {Id = 2, Area = "NW", Address = 3, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
new AddressUnit() {Id = 3, Area = "NW", Address = 5, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
new AddressUnit() {Id = 4, Area = "NW", Address = 7, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0}
});
machine.AddressFormater = new AddressFormaterNA200H();
machine.AddressTranslator = new AddressTranslatorNA200H();
machine.AddressCombiner = new AddressCombinerPercentageJump(20.0);
var result = machine.GetDatas();
var add1 = result["Add1"].PlcValue;
var resultFormat = BaseMachine.MapGetValuesToSetValues(result);
machine.SetDatas(MachineSetDataType.CommunicationTag, resultFormat);
```

To use BaseMachine, follow these steps.

1.New a BaseMachine instance. Remeber BaseMachine is an abstract class.

```C#
BaseMachine machine = new ModbusMachine(ModbusType.Tcp, "192.168.3.12", new List<AddressUnit>()
{
new AddressUnit() {Id = 1, Area = "NW", Address = 1, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
new AddressUnit() {Id = 2, Area = "NW", Address = 3, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
new AddressUnit() {Id = 3, Area = "NW", Address = 5, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
new AddressUnit() {Id = 4, Area = "NW", Address = 7, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0}
});
```
If you want to change address, you can set machine.GetAddresses.

2.You can set AddressFormater, AddressTranslator and AddressCombiner to your own implementation.

```C#
machine.AddressFormater = new AddressFormaterNA200H();
machine.AddressTranslator = new AddressTranslatorNA200H();
machine.AddressCombiner = new AddressCombinerPercentageJump(20.0);
```
AddressFormater can format Area and Address to a string.
AdressTranslator can translate this string to two codes.
AddressCombiner can combiner addresses to groups, one group should send one message to hardware.
There are 4 AddressCombiners implemented in the platform.

1. AddressCombinerSingle: Each address group themself.
2. AddressCombinerContinus: Continually addresses pack to one group. This is the default.
3. AddressCombinerNumericJump: Add some addresses to pack more address in the group, and when the value returns, all added address are ignored.
4. AddressCombinerPercentageJump: Same as AddressCombinerNumericJump, and the added count is calculated by the percentage of all get addresses.

3.Use GetDatas Api.
```C#
var result = machine.GetDatas();
//var result = await machine.GetDatasAsync();
```

4.Retrive data from result.
```C#
var add1 = result["Add1"].PlcValue;
```

5.Format result to SetData format.
```C#
var resultFormat = BaseMachine.MapGetValuesToSetValues(result);
```

6.SetData to machine or another machine.
```C#
machine.SetDatas(MachineSetDataType.CommunicationTag, resultFormat);
```
There is also a SetDatasAsync Api.
machine.SetDatas has two types. It is referenced as the first parameter.

1. MachineSetDataType.Address: the key of the dictionary of the second parameter is address.
2. MachineSetDataType.CommunicationTag: the key of the dictionary of the second parameter is communication tag.

###TaskManager
TaskManager is a high level api that you can manage and control many machines together. Remenber if you want to use this class, all communications must be asyncronized.
Sample of TaskManager calls like this.
```C#
TaskManager task = new TaskManager(2000, true);
List<AddressUnit> addressUnits = new List<AddressUnit>
{
    new AddressUnit() {Id = 0, Area = "V", Address = 1, CommunicationTag = "D1", DataType = typeof (ushort), Zoom = 1},
    new AddressUnit() {Id = 1, Area = "V", Address = 3, CommunicationTag = "D2", DataType = typeof (float), Zoom = 1}
};
task.AddMachine(new SiemensMachine(SiemensType.Tcp, "192.168.3.11",SiemensMachineModel.S7_300, addressUnits, true));
task.ReturnValues += (returnValues) =>
{
     value = new List<string>();
     if (returnValues.Value != null)
     {
         value = from val in returnValues.Value select val.Key + " " + val.Value.PlcValue;
         siemensItems.Dispatcher.Invoke(() => siemensItems.ItemsSource = value);
     }
     else
     {
         Console.WriteLine($"ip {returnValues.Key} not return value");
     }
};
task.TaskStart();
```

To use the TaskManager, use following steps.

1.New A TaskManager instance.
```C#
TaskManager task = new TaskManager(2000, true);
```

2.Add a machine to TaskManager.
```C#
List<AddressUnit> addressUnits = new List<AddressUnit>
{
    new AddressUnit() {Id = 0, Area = "V", Address = 1, CommunicationTag = "D1", DataType = typeof (ushort), Zoom = 1},
    new AddressUnit() {Id = 1, Area = "V", Address = 3, CommunicationTag = "D2", DataType = typeof (float), Zoom = 1}
};
task.AddMachine(new SiemensMachine(SiemensType.Tcp, "192.168.3.11", SiemensMachineModel.S7_300, addressUnits, true));
```

3.Register the ReturnValue Event.
```C#
task.ReturnValues += (returnValues) =>
{
     value = new List<string>();
     if (returnValues.Value != null)
     {
         value = from val in returnValues.Value select val.Key + " " + val.Value.PlcValue;
         siemensItems.Dispatcher.Invoke(() => siemensItems.ItemsSource = value);
     }
     else
     {
         Console.WriteLine($"ip {returnValues.Key} not return value");
     }
};
```
The ReturnValues' key is the machineToken, this sample is "192.168.3.11".
And the value is the same as the machine.GetDatas returns.

4.Start the TaskManager.
```C#
task.TaskStart();
```

5.And don't forget that there is also a SetDatasAsync Api in the TaskManager.
```
public async Task<bool> SetDatasAsync(string connectionToken, MachineSetDataType setDataType, Dictionary<string, double> values)
```