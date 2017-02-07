Modbus.Net.Modbus
===================
[![NuGet](https://img.shields.io/nuget/v/Modbus.Net.Modbus.svg)](https://www.nuget.org/packages/Modbus.Net.Modbus/)

Modbus Implementation of Modbus.Net

Table of Content:
* [Basic Concept](#basic)
* [Address Mapping](#address)
* [Addres Coding](#coding)
* [SubAddress Rules](#subpos)

##<a name="basic"></a> Basic Concept

Modbus is a serial communications protocol originally published by Modicon (now Schneider Electric) in 1979 for use with its programmable logic controllers (PLCs). Simple and robust, it has since become a de facto standard communication protocol, and it is now a commonly available means of connecting industrial electronic devices.(From Wekipedia)

##<a name="address"></a> Address Mapping

Modbus has four types of address: Coil, Discrete Input, Holding Register and Input Register.

Modbus has two address description method: standard and extend.

The following table could show the full address discription in Modbus.

Type             | Standard    | Extend        |
---------------- | ----------- | ------------- |
Coil             | 00001-09999 | 000001-065536 |
Discrete Input   | 10001-19999 | 100001-165536 |
Holding Register | 30001-39999 | 300001-365536 |
Input Register   | 40001-49999 | 400001-465536 |

Standard and Extend address description are all supported in Modbus.Net.Modbus. The only difference is don't write too large number in address. 

The following table shows how to write address in Modbus.Net.Modbus

Standard Modbus Address | Modbus.Net.Modbus String Address |
----------------------- | -------------------------------- |
00001                   | 0X 1                             |
00002                   | 0X 2                             |
09999                   | 0X 9999                          |
065536                  | 0X 65536                         |
10001                   | 1X 1                             |
30001                   | 3X 1                             |
40001                   | 4X 1                             |

##<a name="coding"></a> Address Coding

In Modbus.Net, you can write "0X"(Coil), "1X"(Discrete Input), "3X"(Input Register), "4X"(Holding Register) in Area.

Don't forget to type a space between Area and Address.

If you want to use subpos, type string address like this: 

4X 1.12 (Area Address.Subpos)

##<a name="subpos"></a> SubAddress Rules

For 0X and 1X, the scalation is 1/8. This means each address is bool.

Noticing that you can only read 1X.

SubAddress System is not activated in 0X and 1X.

For 3X and 4X, the scalation is 2. This meas each address is short.

The number of SubAddress is from 0 to 15.

Caution: Modbus.Net.Modbus SubAddress has a giant difference towards standard Modbus.

Bit position from standard modbus is from last to first. But Modbus.Net is from first to last.

Standard Modbus

1  0  1  1  1  0  0  0  1  0  0  0  0  1  1  0
16 15 14 13 12 11 10 9  8  7  6  5  4  3  2  1

Modbus.Net.Modbus

1  0  1  1  1  0  0  0  1  0  0  0  0  1  1  0
0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15