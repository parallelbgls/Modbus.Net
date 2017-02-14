Modbus.Net.OPC
===================
[![NuGet](https://img.shields.io/nuget/v/Modbus.Net.OPC.svg)](https://www.nuget.org/packages/Modbus.Net.OPC/)

Modbus Implementation of Modbus.Net

Table of Content:
* [Basic Concept](#basic)
* [Address Mapping](#address)
* [Link](#link)

##<a name="basic"></a> Basic Concept

Siemens Protocal is derived by Profibus and Profinet.

##<a name="address"></a> Address Mapping

Modbus.Net.OPC has a simple address formatting tool. You can find it from AddressFormaterOPC.

You need to use messages in BaseMachine and AddressUnit to create an OPC tag.

Here is a Sample.

If your tag is "1/15/Value_Opening", 1 is MachineId, 15 is StationId and Value_Opening is point name.

Your tagGeter code should be.

```C#
(baseMachine, addressUnit) => return new string[]{baseMachine.Id, ((XXUnitExtend)addressUnit.unitExtend).stationId, addressUnit.Name};
```

If you want change your tag to "1.15.Value_Opening", just set the seperator to '.' .

##<a name="link"></a> Link

The link of OPC DA should like "opcda://PC-Name/OPC-Software-Name".
