Modbus.Net.Siemens
===================
[![NuGet](https://img.shields.io/nuget/v/Modbus.Net.Siemens.svg)](https://www.nuget.org/packages/Modbus.Net.Siemens/)

Modbus Implementation of Modbus.Net

Table of Content:
* [Basic Concept](#basic)
* [Address Mapping](#address)
* [Addres Coding](#coding)
* [SubAddress Rules](#subpos)

##<a name="basic"></a> Basic Concept

Siemens Protocal is derived by Profibus and Profinet.

##<a name="address"></a> Address Mapping

Modbus.Net.Siemens has two types of AddressMapping -- Modbus.Net way or Siemens way.

But Modbus.Net already decleared the type in AddressUnit, so all of the formatting ways will ignore the address type.

The following table shows the differences between them.

Standard Siemens Address | Modbus.Net Address Format | Siemens Address Format |
------------------------ | ------------------------- | ---------------------- |
I0.0                     | I 0.0                     | I0.0                   |
IB0                      | I 0                       | I0                     |
V10.5                    | V 10.5                    | V10.5                  |
VB19                     | V 19                      | V19                    |
DB1.DBD22                | DB1 22                    | DB1.DB22               |
DB2.DB35.1               | DB2 35.1                  | DB2.DB35.1             |

##<a name="coding"></a> Address Coding

The Coding of Modbus.Net.Siemens is the same as standard siemens protocal.

##<a name="subpos"></a> SubAddress Rules

SubAddress Rules is the same as standard siemens protocal.

Area length will always be 1.