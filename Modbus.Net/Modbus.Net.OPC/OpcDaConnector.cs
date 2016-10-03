using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hylasoft.Opc.Common;
using Hylasoft.Opc.Da;

namespace Modbus.Net.OPC
{
    public class OpcDaConnector : BaseConnector
    {
        public override string ConnectionToken { get; }

        protected bool _connect;
        public override bool IsConnected => _connect;

        protected static Dictionary<string, OpcDaConnector> _instances = new Dictionary<string, OpcDaConnector>();
        protected MyDaClient _daClient;

        protected OpcDaConnector(string host)
        {
            ConnectionToken = host;
        }

        public static OpcDaConnector Instance(string host)
        {
            if (!_instances.ContainsKey(host))
            {
                var connector = new OpcDaConnector(host);
                _instances.Add(host, connector);
            }
            return _instances[host];
        }

        public override bool Connect()
        {
            try
            {
                _daClient = new MyDaClient(new Uri(ConnectionToken));
                _daClient.Connect();
                _connect = true;
                AddInfo("client connected.");
                return true;
            }
            catch (Exception ex)
            {
                AddInfo("client connected exception: " + ex.Message);
                AddInfo("connect failed.");
                _connect = false;
                return false;
            }
        }

        public override Task<bool> ConnectAsync()
        {
            return Task.FromResult(Connect());
        }

        public override bool Disconnect()
        {
            try
            {
                _daClient?.Dispose();
                _daClient = null;
                _connect = false;
                AddInfo("client disconnected successfully.");
                return true;
            }
            catch (Exception ex)
            {
                AddInfo("client disconnected exception: " + ex.Message);
                _connect = false;
                return false;
            }
        }

        public override bool SendMsgWithoutReturn(byte[] message)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> SendMsgWithoutReturnAsync(byte[] message)
        {
            throw new NotImplementedException();
        }

        public override byte[] SendMsg(byte[] message)
        {
            return AsyncHelper.RunSync(() => SendMsgAsync(message));
        }

        public override async Task<byte[]> SendMsgAsync(byte[] message)
        {
            try
            {
                var pos = 0;
                var protocal = BigEndianValueHelper.Instance.GetByte(message, ref pos);
                if (protocal == 0)
                {
                    byte[] tagBytes = new byte[message.Length - 1];
                    Array.Copy(message, 1, tagBytes, 0, tagBytes.Length);
                    string tag = Encoding.UTF8.GetString(tagBytes);
                    var result = await _daClient.ReadAsync(tag);
                    if (result.QualityGood)
                    {
                        return BigEndianValueHelper.Instance.GetBytes(result.Value, result.Value.GetType());
                    }
                    else
                    {
                        return Encoding.ASCII.GetBytes("NoData");
                    }
                }
                else
                {
                    int index = 0;
                    for (int i = 1; i < message.Length - 3; i++)
                    {
                        if (message[i] == 0x00 && message[i + 1] == 0xff && message[i + 2] == 0xff &&
                            message[i + 3] == 0x00)
                        {
                            index = i;
                            break;
                        }
                    }

                    int index2 = 0;
                    for (int i = index + 4; i < message.Length - 3; i++)
                    {
                        if (message[i] == 0x00 && message[i + 1] == 0xff && message[i + 2] == 0xff &&
                            message[i + 3] == 0x00)
                        {
                            index2 = i;
                            break;
                        }
                    }
                    byte[] tagBytes = new byte[index - 1];
                    Array.Copy(message, 1, tagBytes, 0, tagBytes.Length);
                    string tag = Encoding.UTF8.GetString(tagBytes);
                    byte[] typeBytes = new byte[index2 - index - 4];
                    Array.Copy(message, index + 4, typeBytes, 0, typeBytes.Length);
                    Type type = Type.GetType(Encoding.UTF8.GetString(typeBytes));
                    byte[] valueBytes = new byte[message.Length - index2 - 4];
                    Array.Copy(message, index2 + 4, valueBytes, 0, valueBytes.Length);
                    int mainpos = 0, subpos = 0;
                    object value = BigEndianValueHelper.Instance.GetValue(valueBytes, ref mainpos, ref subpos, type);
                    await _daClient.WriteAsync(tag, value);
                    return new byte[] {1};
                }
            }
            catch (Exception e)
            {
                //AddInfo("opc client exception:" + e);
                return Encoding.ASCII.GetBytes("NoData");
                //return null;
            }           
        }

        private void AddInfo(string message)
        {
            Console.WriteLine(message);
        }
    }
}
