Modbus.Net
===================
[![NuGet](https://img.shields.io/nuget/v/Modbus.Net.svg)](https://www.nuget.org/packages/Modbus.Net/)

An hardware communication Library written by C#.

Table of Content:
* [Features](#features)
* [Usage](#usage)
* [Architecture](#architecture)
* [Quick Start](#quick_start)
* [Tutorial](#tutorial)
* [Implement](#implement)
* [Addition](#addition)

## <a name="features"></a> Features
* A open platform that you can easy to extend a industrial communication protocal.
* Modbus Tcp protocal.
* Siemens Tcp protocal (acturally it is the same as Profinet)
* OPC DA protocal.
* All communications could be asynchronized.
* A task manager that you can easily manage multiple connections.
* .NET Framework 4.5 and Visual Studio 2017 support.

## <a name="usage"></a> Usage

### Samples:

There are four samples in the project. All sample project recommand running in Siemens 200 PLC.
PLC Program could be opened by Siemens Portal V13 (Step 7 V13).

Modbus TCP connection and Siemens Ethenet Connection are supported at the same time.

* TripleAdd -- Add three numbers in PLC.
* TaskManager -- Sample usage of TaskManager.
* AnyType -- Get any type in registers.
* CrossLamp -- A sample singal lamp controller.

## <a name="architecture"></a> Architecture

### Connector

Connector implements the basic connecting methods, like Socket, Com and SignalR.

### ProtocalLinker

ProtocalLinker implements the link, send, and send recevice actions.

### ProtocalLinkerBytesExtend

Some Protocal has the same head or tail in the same connection way, but different in different connection way, so bytes extend can extend sending message when they are sended in different type of connection.

### ProtocalUnit

Format and deformat the protocal message between the structual class and bytes array.

### ValueHelper

Help change the value between number and bytes, or number array to bytes array.

### Protocal

Manage all protocals and implement a lazy loading method.

### Utility

Manage several types of Protocal to a same calling interface.

### Machine

Shows the Hardware PLC or other types of machine and implement a high level send and receive api.

### AddressFormater

Format address from definite address to string.

### AddressTranslator

Translate address from string to definite address.

### AddressCombiner

Combine duplicated addresses to organized addresses, each organized addresses communicate once to a device.

### TaskManager

The highest api that you can manage many PLC links and all links are async so there are no block in all connections.

## <a name="quick_start"></a> Quick Start.

The fastest way you can write is to use TaskManager. TaskManager Project is a good example.


```C#
List<AddressUnit> addressUnits = new List<AddressUnit>
{
    new AddressUnit() {Id = "0", Area = "V", Address = 1, CommunicationTag = "D1", DataType = typeof (ushort), Zoom = 1},
    new AddressUnit() {Id = "1", Area = "V", Address = 3, CommunicationTag = "D2", DataType = typeof (float), Zoom = 1}
};
TaskManager task = new TaskManager(10, true);
task.AddMachine(new SiemensMachine(SiemensType.Tcp, "192.168.3.11", SiemensMachineModel.S7_300, addressUnits,
    true, 2, 0));
task.InvokeTimerAll(new TaskItemGetData(returnValues =>
{
    if (returnValues.ReturnValues != null)
    {
        lock (values)
        {
            var unitValues = from val in returnValues.ReturnValues
                select
                new Tuple<AddressUnit, double?>(
                    addressUnits.FirstOrDefault(p => p.CommunicationTag == val.Key), val.Value.PlcValue);
            values = from unitValue in unitValues
                select
                new TaskViewModel()
                {
                    Id = unitValue.Item1.Id,
                    Name = unitValue.Item1.Name,
                    Address = unitValue.Item1.Address.ToString(),
                    Value = unitValue.Item2 ?? 0,
                    Type = unitValue.Item1.DataType.Name
                };
        }
    }
    else
    {
        Console.WriteLine($"ip {returnValues.MachineId} not return value");
    }
}, MachineGetDataType.CommunicationTag, 5000, 60000));
```


Here are the details to use the TaskManager.

1. Initialize the task manager.
Three arguments are: the max tasks you can run in a same time; How long did the next send message call happens(milliseconds); and you should keep the connection when a single message called complete.

2. Add the addresses that you want to communicate to PLC. Area are defined in AddressTranslator in each type of communiction.
Basically you can write only Id, Area, Address, CommunicationTag, DataType and Zoom, and it should work. And there are other fields that you can use.
More important, you can extend and implement your own field in UnitExtend in every AddressUnit, and it will return in return event.

3. Add a machine to TaskManager.
Add a machine like siemens machine to the task manager.

4. Add a TaskItem for one machine or all Machines.
   Modbus.Net implement TaskItemGetDatas and TaskItemSetDatas as the default.

##<a name="tutorial"></a> Tutorial
This platform has three level APIs that you could use: Low level API called "BaseUtility"; Middle level API called "BaseMachine"; High level API called "TaskManager".

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
var result = machine.InvokeMachineMethods<IMachineMethodData>?.GetDatas(MachineGetDataType.CommunicationTag);
var add1 = result["Add1"].PlcValue;
var resultFormat = result.MapGetValuesToSetValues();
machine.InvokeMachineMethods<IMachineMethodData>?.SetDatas(MachineSetDataType.CommunicationTag, resultFormat);
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
var result = machine.InvokeMachineMethods<IMachineMethodData>?.GetDatas(MachineGetDataType.CommunicationTag);
//var result = await machine.InvokeMachineMethods<IMachineMethodData>?.GetDatasAsync(MachineGetDataType.CommunicationTag);
```

4.Retrive data from result.
```C#
var add1 = result["Add1"].PlcValue;
```

5.Format result to SetData parameter.
```C#
var resultFormat = result.MapGetValuesToSetValues();
```

6.SetData to machine or another machine.
```C#
machine.InvokeMachineMethods<IMachineMethodData>?.SetDatas(MachineSetDataType.CommunicationTag, resultFormat);
```
There is also a SetDatasAsync Api.
machine.SetDatas has four types. It is referenced as the first parameter.

1. MachineSetDataType.Address: the key of the dictionary of the second parameter is address.
2. MachineSetDataType.CommunicationTag: the key of the dictionary of the second parameter is communication tag.
3. MachineSetDataType.Id: the key of the dictionary of the second paramenter is ID.
4. MachineSetDataType.Name: the key of the dictionary of the second paramenter is name.

### TaskManager
TaskManager is a high level api that you can manage and control many machines together. Remenber if you want to use this class, all communications must be asyncronized.
Sample of TaskManager calls like this.
```C#
TaskManager task = new TaskManager(10, 2000, true);
List<AddressUnit> addressUnits = new List<AddressUnit>
{
    new AddressUnit() {Id = 0, Area = "V", Address = 1, CommunicationTag = "D1", DataType = typeof (ushort), Zoom = 1},
    new AddressUnit() {Id = 1, Area = "V", Address = 3, CommunicationTag = "D2", DataType = typeof (float), Zoom = 1}
};
task.AddMachine(new SiemensMachine(SiemensType.Tcp, "192.168.3.11",SiemensMachineModel.S7_300, addressUnits, true));
task.InvokeTimerAll(new TaskItemGetData(returnValues =>
{
    if (returnValues.ReturnValues != null)
    {
        lock (values)
        {
            var unitValues = from val in returnValues.ReturnValues
                select
                new Tuple<AddressUnit, double?>(
                    addressUnits.FirstOrDefault(p => p.CommunicationTag == val.Key), val.Value.PlcValue);
            values = from unitValue in unitValues
                select
                new TaskViewModel()
                {
                    Id = unitValue.Item1.Id,
                    Name = unitValue.Item1.Name,
                    Address = unitValue.Item1.Address.ToString(),
                    Value = unitValue.Item2 ?? 0,
                    Type = unitValue.Item1.DataType.Name
                };
        }
    }
    else
    {
        Console.WriteLine($"ip {returnValues.MachineId} not return value");
    }
}, MachineGetDataType.CommunicationTag, 5000, 60000));
```

To use the TaskManager, use following steps.

1.New A TaskManager instance.
```C#
TaskManager task = new TaskManager(10, 2000, true);
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

3.Add a get value cycling task. You can handle return values with your own code (ReturnValue event before 1.3.2).
```C#
task.InvokeTimerAll(new TaskItemGetData(returnValues =>
{
    if (returnValues.ReturnValues != null)
    {
        lock (values)
        {
            var unitValues = from val in returnValues.ReturnValues
                select
                new Tuple<AddressUnit, double?>(
                    addressUnits.FirstOrDefault(p => p.CommunicationTag == val.Key), val.Value.PlcValue);
            values = from unitValue in unitValues
                select
                new TaskViewModel()
                {
                    Id = unitValue.Item1.Id,
                    Name = unitValue.Item1.Name,
                    Address = unitValue.Item1.Address.ToString(),
                    Value = unitValue.Item2 ?? 0,
                    Type = unitValue.Item1.DataType.Name
                };
        }
    }
    else
    {
        Console.WriteLine($"ip {returnValues.MachineId} not return value");
    }
}, MachineGetDataType.CommunicationTag, 5000, 60000));
```

5.And don't forget that there is also a SetDatasAsync Api in the TaskManager.
```C#
public async Task<bool> SetDatasAsync(string connectionToken, MachineSetDataType setDataType, Dictionary<string, double> values)
```

##<a name="implement"></a> Implementing Your Own Protocal
The main target of Modbus.Net is building a high extensable hardware communication protocal, so we allow everyone to extend the protocal.

To extend Modbus.Net, first of all ValueHelper.cs in Modbus.Net is a really powerful tool that you can use to modify values in byte array.There are two ValueHelpers: ValueHelper(Little Endian) and BigEndianValueHelper(Big Endian). Remember using the correct one.

In this tutorial I will use Modbus.Net.Modbus to tell you how to implement your own protocal.

You should follow the following steps to implement your own protocal.

1.Implement Protocal. (ModbusProtocal.cs, ModbusTcpProtocal.cs)
First: Extend BaseProtocal to ModbusProtocal.
```C#
public abstract class ModbusProtocal : BaseProtocal
public class ModbusTcpProtocal : ModbusProtocal
```
"abstract" keyword is optional because if user can use this protocal don't write abstract.

Second: Extend ProtocalUnit, IInputStruct and IOutputStruct.
```C#
public class ReadDataModbusProtocal : ProtocalUnit
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
There is another attribute called SpecialProtocalUnitAttribute.
If you add SpecialProtocalUnitAttribute to ProtocalUnit, then the protocal will not run BytesExtend and BytesDecact.
```C#
[SpecialProtocalUnit]
internal class CreateReferenceSiemensProtocal : ProtocalUnit
{
    ...
}
```

2.Implement Protocal based ProtocalLinker. (ModbusTcpProtocalLinker)
ProtocalLinker connect the Protocal to the BaseConnector, so that byte array can be sended using some specific way like Ethenet.
```C#
public class ModbusTcpProtocalLinker : TcpProtocalLinker
{
    public override bool CheckRight(byte[] content)
```
CheckRight is the return check function, if you got the wrong bytes answer, you can return false or throw exceptions.

3.Implement Connector based ProtocalLinker and BaseConnector. (TcpProtocalLinker.cs, TcpConnector.cs) (Optional)
If you want to connect to hardware using another way, please implement your own ProtocalLinker and BaseConnector. And please remember that if you want to connector hardware using serial line, please don't use ComConnector in Modbus.Net and implement your own ComConnector.
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

4.Implement ProtocalLinkerBytesExtend (ModbusProtocalLinkerBytesExtend.cs)
If you want to use extend bytes when you send your bytes array to the hardware, you can set ProtocalLinkerBytesExtend.
The name of ProtocalLinkerBytesExtend is ProtocalLinker name + BytesExtend, like ModbusTcpProtocalLinkerBytesExtend.
```C#
public class ModbusTcpProtocalLinkerBytesExtend : ProtocalLinkerBytesExtend
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
For example modbus tcp has a 6 bytes head: 4 bytes 0 and 2 bytes length. And when you get the bytes, please remove the head to fit the ModbusProtocal Unformat function.

5.Implement BaseUtility.cs (ModbusUtility.cs)
Implement low level api for Modbus.
You need to implement three functions.
```C#
public override void SetConnectionType(int connectionType)
protected override async Task<byte[]> GetDatasAsync(byte slaveAddress, byte masterAddress, string startAddress, int getByteCount)
public override async Task<bool> SetDatasAsync(byte slaveAddress, byte masterAddress, string startAddress, object[] setContents)
```
And don't remember set default AddressTranslator, slaveAddress, masterAddress and Protocal.
```C#
public ModbusUtility(int connectionType, byte slaveAddress, byte masterAddress) : base(slaveAddress, masterAddress)
{
    ConnectionString = null;
    ModbusType = (ModbusType)connectionType;
    AddressTranslator = new AddressTranslatorModbus();
}
```

6.Implement BaseMachine.cs (ModbusMachine.cs)
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

7.Implement your own AddressFormater, AddressTranslator and AddressCombiner. (AddressFormaterModbus.cs, AddressTranslatorModbus.cs) (Optional)
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
First of all, there are two types of coordinates in Modbus.Net Address System - Protocal Coordinate and Abstract Coordinate.

Here is an example of the differences between them:

In Register of Modbus, the minimum type is short, but Modbus.Net use type of byte to show the result. If you want to get value from 400003 in protocal coordinate, you actually get 5th and 6th byte value in abstract coordinate.

Version 1.0 and 1.1 used abstract coordinate so you need to convert the address. But fortunatly after 1.2, AddressUnit uses Protocal Coordinate so that you donnot need the convert the address descripted by modbus protocal itself, but this means if you want to get a bit or byte value in modbus, you need to use the subpos system.

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
