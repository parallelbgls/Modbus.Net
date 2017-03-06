using Hylasoft.Opc.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    public abstract class OpcConnector : BaseConnector
    {
        protected IClientExtend Client;

        protected bool _connect;
        public override string ConnectionToken { get; }
        public override bool IsConnected => _connect;

        protected OpcConnector(string host)
        {
            ConnectionToken = host;
        }     

        public override bool Disconnect()
        {
            try
            {
                Client?.Dispose();
                Client = null;
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
                    var tagBytes = new byte[message.Length - 1];
                    Array.Copy(message, 1, tagBytes, 0, tagBytes.Length);
                    var tag = Encoding.UTF8.GetString(tagBytes);
                    var tagSplit = tag.Split('/');
                    var rootDirectory = await Client.ExploreFolderAsync("/");
                    var answerTag = await SearchTag(tagSplit, 0, rootDirectory);
                    if (answerTag != null)
                    {
                        var result = await Client.ReadAsync<object>(answerTag);
                        return BigEndianValueHelper.Instance.GetBytes(result, result.GetType());
                    }
                    return Encoding.ASCII.GetBytes("NoData");
                }
                else
                {
                    var index = 0;
                    for (var i = 1; i < message.Length - 3; i++)
                    {
                        if (message[i] == 0x00 && message[i + 1] == 0xff && message[i + 2] == 0xff &&
                            message[i + 3] == 0x00)
                        {
                            index = i;
                            break;
                        }
                    }

                    var index2 = 0;
                    for (var i = index + 4; i < message.Length - 3; i++)
                    {
                        if (message[i] == 0x00 && message[i + 1] == 0xff && message[i + 2] == 0xff &&
                            message[i + 3] == 0x00)
                        {
                            index2 = i;
                            break;
                        }
                    }
                    var tagBytes = new byte[index - 1];
                    Array.Copy(message, 1, tagBytes, 0, tagBytes.Length);
                    var tag = Encoding.UTF8.GetString(tagBytes);
                    var typeBytes = new byte[index2 - index - 4];
                    Array.Copy(message, index + 4, typeBytes, 0, typeBytes.Length);
                    var type = Type.GetType(Encoding.UTF8.GetString(typeBytes));
                    var valueBytes = new byte[message.Length - index2 - 4];
                    Array.Copy(message, index2 + 4, valueBytes, 0, valueBytes.Length);
                    int mainpos = 0, subpos = 0;
                    var value = BigEndianValueHelper.Instance.GetValue(valueBytes, ref mainpos, ref subpos, type);
                    await Client.WriteAsync(tag, value);
                    return new byte[] { 1 };
                }
            }
            catch (Exception e)
            {
                //AddInfo("opc client exception:" + e);
                return Encoding.ASCII.GetBytes("NoData");
                //return null;
            }
        }

        private async Task<string> SearchTag(string[] tags, int deep, IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var currentTag = node.Tag.Substring(node.Tag.LastIndexOf('/'));
                if (Regex.IsMatch(currentTag, tags[deep]))
                {
                    if (deep == tags.Length) return currentTag;
                    var subDirectories = await Client.ExploreFolderAsync(node.Tag);
                    var answerTag = await SearchTag(tags, deep + 1, subDirectories);
                    if (answerTag != null) return answerTag;
                }
            }
            return null;
        }

        protected void AddInfo(string message)
        {
            Console.WriteLine(message);
        }

        public override bool Connect()
        {
            try
            {
                Client.Connect();
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
    }
}
