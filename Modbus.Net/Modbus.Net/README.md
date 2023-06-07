Modbus.Net
===================
[![NuGet](https://img.shields.io/nuget/v/Modbus.Net.svg)](https://www.nuget.org/packages/Modbus.Net/)

An hardware communication Library written by C#.

Table of Content:
* [License Description](#license)
* [Features](#features)
* [Usage](#usage)
* [Architecture](#architecture)
* [Quick Start](#quick_start)
* [Tutorial](#tutorial)
* [Implement](#implement)
* [Addition](#addition)

## <a name="license"></a> License Description
This project uses MIT license. This means you can alter any codes, use in your project without any declaration.

## <a name="features"></a> Features
* A open platform that you can easily extend a industrial communication protocol.
* Modbus Tcp protocol.
* Siemens Tcp protocol (acturally it is the same as Profinet)
* All communications could be asynchronized.
* A task manager that you can easily manage multiple connections.
* .NET 6.0 support.

## <a name="architecture"></a> Architecture

### Controller

Controller implements the basic message control methods, like FIFO.

### Connector

Connector implements the basic connecting methods, like Socket, Com.

### ProtocolLinker

ProtocolLinker implements the link, send, and send recevice actions.

### ProtocolLinkerBytesExtend

Some Protocol has the same head or tail in the same connection way, but different in different connection way, so bytes extend can extend sending message when they are sended in different type of connection.

### ProtocolUnit

Format and deformat the protocol message between the structual class and bytes array.

### ValueHelper

Help change the value between number and bytes, or number array to bytes array.

### Protocol

Manage all protocols and implement a lazy loading method.

### Utility

Manage several types of Protocol to a same calling interface.

### Machine

Shows the Hardware PLC or other types of machine and implement a high level send and receive api.

### Job

A job implementation by Quartz.

### AddressFormater

Format address from definite address to string.

### AddressTranslator

Translate address from string to definite address.

### AddressCombiner

Combine duplicated addresses to organized addresses, each organized addresses communicate once to a device.

## <a name="tutorial"></a> Tutorial

This platform has three level APIs that you could use: Low level API called "BaseUtility"; Middle level API called "BaseMachine"

### Utility

IUtilityProperty is a low level api, in this level you can get or set data only by byte array or object array. Here is an example.

```C#
string ip = "192.168.0.10";
IUtilityProperty utility = new ModbusUtility(ModbusType.Tcp, ip, 0x02, 0x00);
object[] getNum = utility.InvokeUtilityMethod<IUtilityMethodData>?.GetDatas("4X 1", new KeyValuePair<Type, int>(typeof(ushort), 4));
```

BaseUtility is an abstract class. You can check all apis in BaseUtility.cs in Modbus.Net project.

To use BaseUtility, follow these steps.

1.New a BaseUtility instance, but remember BaseUtility is an abstract class, you should new class inherit from it.
```C#
IUtilityPropety utility = new ModbusUtility(ModbusType.Tcp, ip, 0x02, 0x00);
```

2.Use GetData and SetData Api in IUtilityMethodData, like
```C#
object[] getNum = utility.InvokeUtilityMethod<IUtilityMethodData>?.GetDatas("4X 1", new KeyValuePair<Type, int>(typeof(ushort), 4));

utility.InvokeUtilityMethod<IUtilityMethodData>?.SetDatas("4X 1", new object[] { (ushort)1, (ushort)2, (ushort)3 });
```
Remember force set type of numbers because GetData and SetData Apis are type sensitive.

You can also use async functions like
```C#
object[] getNum = await utility.InvokeUtilityMethod<IUtilityMethodData>?.GetDatasAsync("4X 1", new KeyValuePair<Type, int>(typeof(ushort), 4));
```

### Machine

IMachineProperty is a middle level api, in this level you could get and set datas in a managable data structure for a single machine.
To understand this class, you have to see the AddressUnit first.
```C#
public class AddressUnit
{
    public string Id { get; set; }
    public string Area { get; set; }
    public int Address { get; set; }
    public int SubAddress { get; set; } = 0;
    public Type DataType { get; set; }
    public double Zoom { get; set; }
    public int DecimalPos { get; set; }
    public string CommunicationTag { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public bool CanWrite { get; set; } = true;
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

Then using IMachineProperty like this.
```C#
IMachineProperty machine = new ModbusMachine(ModbusType.Tcp, "192.168.3.12", new List<AddressUnit>()
{
machine = new ModbusMachine(ModbusType.Rtu, "COM3", new List<AddressUnit>()
{
    new AddressUnit() {Id = "1", Area = "4X", Address = 1, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
    new AddressUnit() {Id = "2", Area = "4X", Address = 2, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
    new AddressUnit() {Id = "3", Area = "4X", Address = 3, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
    new AddressUnit() {Id = "4", Area = "4X", Address = 4, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
}, 2, 0);
machine.AddressCombiner = new AddressCombinerContinus(machine.AddressTranslator);
machine.AddressCombinerSet = new AddressCombinerContinus(machine.AddressTranslator);
machine.AddressCombiner = new AddressCombinerPercentageJump(20.0);
var result = machine.InvokeMachineMethods<IMachineMethodData>?.GetDatas(MachineDataType.CommunicationTag);
var add1 = result["Add1"].DeviceValue;
var resultFormat = result.MapGetValuesToSetValues();
machine.InvokeMachineMethods<IMachineMethodData>?.SetDatas(MachineDataType.CommunicationTag, resultFormat);
```

To use BaseMachine, follow these steps.

1.New a IMachineProperty instance.

```C#
IMachineProperty machine = new ModbusMachine(ModbusType.Rtu, "COM3", new List<AddressUnit>()
{
    new AddressUnit() {Id = "1", Area = "4X", Address = 1, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
    new AddressUnit() {Id = "2", Area = "4X", Address = 2, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
    new AddressUnit() {Id = "3", Area = "4X", Address = 3, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
    new AddressUnit() {Id = "4", Area = "4X", Address = 4, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
}, 2, 0);
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
var result = machine.InvokeMachineMethods<IMachineMethodDatas>?.GetDatas(MachineDataType.CommunicationTag);
//var result = await machine.InvokeMachineMethods<IMachineMethodDatas>?.GetDatasAsync(MachineDataType.CommunicationTag);
```

4.Retrive data from result.
```C#
var add1 = result["Add1"].DeviceValue;
```

5.Format result to SetData parameter.
```C#
var resultFormat = result.MapGetValuesToSetValues();
```

6.SetData to machine or another machine.
```C#
machine.InvokeMachineMethods<IMachineMethodData>?.SetDatas(MachineDataType.CommunicationTag, resultFormat);
```
There is also a SetDatasAsync Api.
machine.SetDatas has four types. It is referenced as the first parameter.

1. MachineDataType.Address: the key of the dictionary of the second parameter is address.
2. MachineDataType.CommunicationTag: the key of the dictionary of the second parameter is communication tag.
3. MachineDataType.Id: the key of the dictionary of the second paramenter is ID.
4. MachineDataType.Name: the key of the dictionary of the second paramenter is name.

### Job

You can use MachineJobSchedulerCreator to create a job scheduler then write a job chain and run this chain.
```C#
var scheduler = await MachineJobSchedulerCreator<IMachineMethodDatas, string, double>.CreateScheduler(machine.Id, -1, 10);
var job = scheduler.From(machine.Id + ".From", machine, MachineDataType.Name).Result.Query(machine.Id + ".ConsoleQuery", QueryConsole).Result.To(machine.Id + ".To", machine).Result.Deal(machine.Id + ".Deal", OnSuccess, OnFailure).Result;
await job.Run();
```

Also you can use MultipleMachinesJobScheduler to run multiple machines in a same chain.
```C#
MultipleMachinesJobScheduler.RunScheduler(machines, async (machine, scheduler) =>
{
    await scheduler.From(machine.Id + ".From", machine, MachineDataType.Name).Result.Query(machine.Id + ".ConsoleQuery", QueryConsole).Result.To(machine.Id + ".To", machine).Result.Deal(machine.Id + ".Deal", OnSuccess, OnFailure).Result.Run();
}, -1, 10)
```

### Read Machine Parameter from appsettings.json

First writing C# Code to read machines.
```C#
var machines = MachineReader.ReadMachines();
```
Then writing json config in appsettings.json
```Json
"Machine": [
      {
        "a:id": "ModbusMachine1",
        "b:protocol": "Modbus",
        "c:type": "Tcp",
        "d:connectionString": "10.10.18.251",
        "e:addressMap": "AddressMapModbus",
        "f:keepConnect": true,
        "g:slaveAddress": 1,
        "h:masterAddress": 2,
        "i:endian": "BigEndianLsb"
      },
...
]
"addressMap": {
      "AddressMapModbus": [
        {
          "Area": "4X",
          "Address": 1,
          "DataType": "Int16",
          "Id": "1",
          "Name": "Test1"
        },
...
      ],
...
}
```
For some reasons, you need to add e.g. "a:" "b:" to let property ordered in machine configuration, anything can be used here before ":".
But after ":", property should match constructor except protocol, which refer to class name.

## <a name="implement"></a> Implementing Your Own Protocol

The main target of Modbus.Net is building a high extensable hardware communication protocol, so we allow everyone to extend the protocol.

To extend Modbus.Net, first of all ValueHelper.cs in Modbus.Net is a really powerful tool that you can use to modify values in byte array.There are two ValueHelpers: ValueHelper(Little Endian) and BigEndianValueHelper(Big Endian). Remember using the correct one.

In this tutorial I will use Modbus.Net.Modbus to tell you how to implement your own protocol.

You should follow the following steps to implement your own protocol.

1.Implement Protocol. (ModbusProtocol.cs, ModbusTcpProtocol.cs)
First: Extend BaseProtocol to ModbusProtocol.
```C#
public abstract class ModbusProtocol : BaseProtocol
public class ModbusTcpProtocol : ModbusProtocol
```
"abstract" keyword is optional because if user can use this protocol don't write abstract.

Second: Extend ProtocolUnit, IInputStruct and IOutputStruct.
```C#
public class ReadDataModbusProtocol : ProtocolUnit
{
    public override byte[] Format(IInputStruct message)
    {
        var r_message = (ReadDataModbusInputStruct)message;
        return Format(r_message.SlaveAddress, r_message.FunctionCode, r_message.StartAddress, r_message.GetCount);
    }

    public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
    {
        byte slaveAddress = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
        byte functionCode = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
        byte dataCount = BigEndianValueHelper.Instance.GetByte(messageBytes, ref pos);
        byte[] dataValue = new byte[dataCount];
        Array.Copy(messageBytes, 3, dataValue, 0, dataCount);
        return new ReadDataModbusOutputStruct(slaveAddress, functionCode, dataCount, dataValue);
    }
}
```
There is another attribute called SpecialProtocolUnitAttribute.
If you add SpecialProtocolUnitAttribute to ProtocolUnit, then the protocol will not run BytesExtend and BytesDecact.
```C#
[SpecialProtocolUnit]
internal class CreateReferenceSiemensProtocol : ProtocolUnit
{
    ...
}
```

2.Implement Protocol based ProtocolLinker. (ModbusTcpProtocolLinker)
ProtocolLinker connect the Protocol to the BaseConnector, so that byte array can be sended using some specific way like Ethenet.
```C#
public class ModbusTcpProtocolLinker : TcpProtocolLinker
{
    public override bool CheckRight(byte[] content)
```
CheckRight is the return check function, if you got the wrong bytes answer, you can return false or throw exceptions.

3.Implement Connector based ProtocolLinker and BaseConnector. (TcpProtocolLinker.cs, TcpConnector.cs) (Optional)
If you want to connect to hardware using another way, please implement your own ProtocolLinker and BaseConnector. And please remember that if you want to connector hardware using serial line, please don't use ComConnector in Modbus.Net and implement your own ComConnector.
You should implement all of these functions in BaseConnector.
```C#
public abstract string ConnectionToken { get; }
public abstract bool IsConnected { get; }
public abstract bool Connect();
public abstract Task<bool> ConnectAsync();
public abstract bool Disconnect();
public abstract bool SendMsgWithoutReturn(byte[] message);
public abstract Task<bool> SendMsgWithoutReturnAsync(byte[] message);
public abstract byte[] SendMsg(byte[] message);
public abstract Task<byte[]> SendMsgAsync(byte[] message);
```

4.Implement ProtocolLinkerBytesExtend (ModbusProtocolLinkerBytesExtend.cs)
If you want to use extend bytes when you send your bytes array to the hardware, you can set ProtocolLinkerBytesExtend.
The name of ProtocolLinkerBytesExtend is ProtocolLinker name + BytesExtend, like ModbusTcpProtocolLinkerBytesExtend.
```C#
public class ModbusTcpProtocolLinkerBytesExtend : ProtocolLinkerBytesExtend
{
    public override byte[] BytesExtend(byte[] content)
    {        
        byte[] newFormat = new byte[6 + content.Length];
        int tag = 0;
        ushort leng = (ushort)content.Length;
        Array.Copy(BigEndianValueHelper.Instance.GetBytes(tag), 0, newFormat, 0, 4);
        Array.Copy(BigEndianValueHelper.Instance.GetBytes(leng), 0, newFormat, 4, 2);
        Array.Copy(content, 0, newFormat, 6, content.Length);
        return newFormat;
    }

    public override byte[] BytesDecact(byte[] content)
    {
        byte[] newContent = new byte[content.Length - 6];
        Array.Copy(content, 6, newContent, 0, newContent.Length);
        return newContent;
    }
}
```
For example modbus tcp has a 6 bytes head: 4 bytes 0 and 2 bytes length. And when you get the bytes, please remove the head to fit the ModbusProtocol Unformat function.

5.Implement BaseController.cs (FIFOController.cs)
Implement message dispatching api like first in first out.
There are no rules for implementation, but you can refer IController and FIFOController to implement your own controller like RBTreeController.

6.Implement BaseUtility.cs (ModbusUtility.cs)
Implement low level api for Modbus.
You need to implement three functions.
```C#
public override void SetConnectionType(int connectionType)
protected override async Task<byte[]> GetDatasAsync(byte slaveAddress, byte masterAddress, string startAddress, int getByteCount)
public override async Task<bool> SetDatasAsync(byte slaveAddress, byte masterAddress, string startAddress, object[] setContents)
```
And don't remember set default AddressTranslator, slaveAddress, masterAddress and Protocol.
```C#
public ModbusUtility(int connectionType, byte slaveAddress, byte masterAddress) : base(slaveAddress, masterAddress)
{
    ConnectionString = null;
    ModbusType = (ModbusType)connectionType;
    AddressTranslator = new AddressTranslatorModbus();
}
```

7.Implement BaseMachine.cs (ModbusMachine.cs)
Implement middle level api for Modbus.
```C#
public ModbusMachine(ModbusType connectionType, string connectionString,
	IEnumerable<AddressUnit> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress)
	: base(getAddresses, keepConnect, slaveAddress, masterAddress)
{
	BaseUtility = new ModbusUtility(connectionType, connectionString, slaveAddress, masterAddress);
	AddressFormater = new AddressFormaterModbus();
	AddressCombiner = new AddressCombinerContinus(AddressTranslator);
	AddressCombinerSet = new AddressCombinerContinus(AddressTranslator);
}
```
Set BaseUtility, default AddressFormater, AddressCombiner and AddressCombinerSet.

8.Implement your own AddressFormater, AddressTranslator and AddressCombiner. (AddressFormaterModbus.cs, AddressTranslatorModbus.cs) (Optional)
If some devices have its own address rule, you should implement your own address formating system.
```C#
public class AddressFormaterModbus : AddressFormater
{
    public override string FormatAddress(string area, int address)
    {
        return area + " " + address;
    }

    public override string FormatAddress(string area, int address, int subAddress)
    {
        return area + " " + address  + "." + subAddress;
    }
}
```

## <a name="addition"></a> Addition

### For Subaddress System
Subaddress system is implemented for reading and writing of bits.
```C#
public class AddressUnit
{
    public int SubAddress { get; set; } = 0;
}
```
First of all, there are two types of coordinates in Modbus.Net Address System - Protocol Coordinate and Abstract Coordinate.

Here is an example of the differences between them:

In Register of Modbus, the minimum type is short, but Modbus.Net use type of byte to show the result. If you want to get value from 400003 in protocol coordinate, you actually get 5th and 6th byte value in abstract coordinate.

Version 1.0 and 1.1 used abstract coordinate so you need to convert the address. But fortunatly after 1.2, AddressUnit uses Protocol Coordinate so that you donnot need the convert the address descripted by modbus protocol itself, but this means if you want to get a bit or byte value in modbus, you need to use the subpos system.

For example if you want the get a value from the 6th byte in Hold Register. In traditional modbus you can only get 400003 of 2 bytes and get the 2nd byte from it. But in Modbus.Net there is an easy way to get it.
```
Modbus 400003 2nd byte 
```
Donnot wonder I use real number to implement subpos counter. There are no tolerances because only (1/2 pow n) could be used to count subpos.

```C#

class AddressUnit
{
Area = "4X"
Address = 3
SubAddress = 8
type = typeof(byte)
}
```

SubAddress 8 means it starts from the 8th bit in that short value.

Remember subpos system cannot cross a byte in current version. If you want to cross a byte, you can change the function "GetValue" in ValueHelper.cs 

### For configurations

You can replace any configuration in appsettings.default.json, and remember, you can change settings only for one connection or one physical port.
Like
```Json
{
    "Modbus.Net:TCP:192.168.1.100:502:FetchSleepTime": 50,
    "Modbus.Net:TCP:192.168.1.101:FetchSleepTime": 50,
    "Modbus.Net:COM:COM1:FetchSleepTime": 2000
}
```
