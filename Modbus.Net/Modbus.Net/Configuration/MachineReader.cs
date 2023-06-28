using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Modbus.Net
{
    /// <summary>
    ///     从配置文件读取设备列表
    /// </summary>
    public class MachineReader
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.default.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
            .Build();

        /// <summary>
        ///     读取设备列表
        /// </summary>
        /// <returns>设备的列表</returns>
        public static List<IMachine<string>> ReadMachines()
        {
            var ans = new List<IMachine<string>>();
            var root = configuration.GetSection("Modbus.Net").GetSection("Machine").GetChildren();
            foreach (var machine in root)
            {
                List<KeyValuePair<string, string>> kv = new List<KeyValuePair<string, string>>();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (var paramO in machine.GetChildren())
                {
                    foreach (var param in paramO.GetChildren())
                    {
                        kv.Add(new KeyValuePair<string, string>(param.Key, param.Value));
                        dic[param.Key] = param.Value;
                    }
                }

                List<object> paramsSet = new List<object>();
                foreach (var param in kv)
                {
                    switch (param.Key)
                    {
                        case "protocol":
                            {
                                paramsSet.Add(Enum.Parse(Assembly.Load("Modbus.Net." + dic["protocol"]).GetType("Modbus.Net." + dic["protocol"] + "." + dic["protocol"] + "Type"), dic["type"]));
                                break;
                            }
                        case "type":
                            {
                                break;
                            }
                        case "model":
                            {
                                paramsSet.Add(Enum.Parse(Assembly.Load("Modbus.Net." + dic["protocol"]).GetType("Modbus.Net." + dic["protocol"] + "." + dic["protocol"] + "MachineModel"), dic["model"]));
                                break;
                            }
                        case "addressMap":
                            {
                                var machineTypeTemp = Assembly.Load("Modbus.Net." + dic["protocol"]).GetType("Modbus.Net." + dic["protocol"] + "." + dic["protocol"] + "Machine`2");
                                var addressTypes = machineTypeTemp.GetProperty("GetAddresses").PropertyType.GenericTypeArguments[0].GenericTypeArguments;
                                if (addressTypes[1] == typeof(int) && addressTypes[2] == typeof(int))
                                {
                                    paramsSet.Add(AddressReader<string, int, int>.ReadAddresses(dic["addressMap"]));
                                    break;
                                }
                                else if (addressTypes[1] == typeof(string) && addressTypes[2] == typeof(string))
                                {
                                    paramsSet.Add(AddressReader<string, string, string>.ReadAddresses(dic["addressMap"]));
                                    break;
                                }
                                throw new NotSupportedException("AddressUnit type not supported for AddressReader");
                            }
                        case "endian":
                            {
                                paramsSet.Add(Endian.Parse(dic["endian"]));
                                break;
                            }
                        default:
                            {
                                string value = param.Value;
                                bool boolValue;
                                byte byteValue;
                                if (!bool.TryParse(value, out boolValue))
                                {
                                    if (!byte.TryParse(value, out byteValue))
                                    {
                                        paramsSet.Add(value);
                                    }
                                    else
                                    {
                                        paramsSet.Add(byteValue);
                                    }
                                }
                                else
                                {
                                    paramsSet.Add(boolValue);
                                }
                                break;
                            }
                    }

                }
                Type machineType = Assembly.Load("Modbus.Net." + dic["protocol"]).GetType("Modbus.Net." + dic["protocol"] + "." + dic["protocol"] + "Machine`2");
                Type[] typeParams = new Type[] { typeof(string), typeof(string) };
                Type constructedType = machineType.MakeGenericType(typeParams);
                IMachine<string> machineInstance = Activator.CreateInstance(constructedType, paramsSet.ToArray()) as IMachine<string>;
                ans.Add(machineInstance);
            }
            return ans;
        }
    }

    internal class AddressReader<TUnitKey, TAddressKey, TSubAddressKey> where TUnitKey : IEquatable<TUnitKey> where TAddressKey : IEquatable<TAddressKey> where TSubAddressKey : IEquatable<TSubAddressKey>
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.default.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
            .Build();

        public static IEnumerable<AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>> ReadAddresses(string addressMapName)
        {
            var ans = new List<AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>>();
            var addressesRoot = configuration.GetSection("Modbus.Net").GetSection("AddressMap").GetSection(addressMapName).GetChildren();
            foreach (var address in addressesRoot)
            {

                var addressNew = address.Get<AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>>();
                addressNew.DataType = "System." + address["DataType"] != null ? Type.GetType("System." + address["DataType"]) : throw new ArgumentNullException("DataType is null");
                if (addressNew.DataType == null) throw new ArgumentNullException(string.Format("DataType define error {0} {1} {2}", addressMapName, addressNew.Id, address["DataType"]));
                ans.Add(addressNew);
            }
            return ans.AsEnumerable();
        }
    }
}
