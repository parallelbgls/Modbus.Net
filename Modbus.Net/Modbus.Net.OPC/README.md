Modbus.Net.OPC
===================
[![NuGet](https://img.shields.io/nuget/v/Modbus.Net.OPC.svg)](https://www.nuget.org/packages/Modbus.Net.OPC/)

Modbus Implementation of Modbus.Net

Table of Content:
* [Basic Concept](#basic)
* [Address Mapping](#address)
* [Regex System](#regex)
* [Link](#link)

## <a name="basic"></a> Basic Concept

OPC Protocal implements OPC DA and OPC UA protocal.

## <a name="address"></a> Address Mapping

Modbus.Net.OPC has a simple address formatting tool. You can find it from AddressFormaterOPC.

You need to use messages in BaseMachine and AddressUnit to create an OPC tag.

Here is a Sample.

If your tag is "1/15/Value_Opening", 1 is MachineId, 15 is StationId and Value_Opening is point name.

Your tagGeter code should be.

```C#
(baseMachine, addressUnit) => return new string[]{baseMachine.Id, ((XXUnitExtend)addressUnit.unitExtend).stationId, addressUnit.Name};
```

If you want change your tag to "1.15.Value_Opening", just set the seperator to '.' .

## <a name="regex"></a> Regex System

Every tag could be a regex expression, Modbus.Net.OPC will always use regex comparison mode between each tags.

Like if you want to use a wildcard for first tag(or first directory), input your tagGeter code like this:

```C#
(baseMachine, addressUnit) => return new string[]{"(.*)", baseMachine.Id, ((XXUnitExtend)addressUnit.unitExtend).stationId, addressUnit.Name};
```

## <a name="link"></a> Link

The link of OPC DA should like "opcda://PC-Name/OPC-Software-Name".

The link of OPC UA should like "opc.tcp://PC-Name/Opc-Software-Name".
