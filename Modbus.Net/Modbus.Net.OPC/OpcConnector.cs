using Hylasoft.Opc.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    public abstract class OpcConnector : BaseConnector<OpcParamIn, OpcParamOut>
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

        public override bool SendMsgWithoutReturn(OpcParamIn message)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> SendMsgWithoutReturnAsync(OpcParamIn message)
        {
            throw new NotImplementedException();
        }

        public override OpcParamOut SendMsg(OpcParamIn message)
        {
            return AsyncHelper.RunSync(() => SendMsgAsync(message));
        }

        private void FoldWith(List<string> tagSplitList, char splitChar, char startChar, char endChar)
        {
            for (int i = 0; i < tagSplitList.Count; i++)
            {
                if (tagSplitList[i].Count(ch=>ch == startChar) > tagSplitList[i].Count(ch=>ch == endChar))
                {
                    for (int j = i + 1; j < tagSplitList.Count; j++)
                    {
                        if (tagSplitList[j].Contains(endChar))
                        {
                            for (int k = i + 1; k <= j; k++)
                            {
                                tagSplitList[i] += splitChar + tagSplitList[i + 1];
                                tagSplitList.RemoveAt(i + 1);
                            }
                            i--;
                            break;
                        }
                    }
                }
            }
        }

        private string[] SplitTag(string tag, char split)
        {
            var tagSplitList = tag.Split(split).ToList();

            FoldWith(tagSplitList, split, '(', ')');
            FoldWith(tagSplitList, split, '[', ']');
            FoldWith(tagSplitList, split, '{', '}');

            return tagSplitList.ToArray();
        }

        public override async Task<OpcParamOut> SendMsgAsync(OpcParamIn message)
        {
            try
            {
                if (message.IsRead)
                {
                    var split = message.Split;
                    var tag = message.Tag;
                    var tagSplit = SplitTag(tag, split);
                    var rootDirectory = await Client.ExploreFolderAsync("");
                    var answerTag = await SearchTag(tagSplit, split, 0, rootDirectory);
                    if (answerTag != null)
                    {
                        var result = await Client.ReadAsync<object>(answerTag);
                        return new OpcParamOut
                        {
                            Success = true,
                            Value = BigEndianValueHelper.Instance.GetBytes(result, result.GetType())
                        };
                    }
                    return new OpcParamOut()
                    {
                        Success = false,
                        Value = Encoding.ASCII.GetBytes("NoData")
                    };
                }
                else
                {
                    var tag = message.Tag;
                    var split = message.Split;
                    var value = message.SetValue;

                    var rootDirectory = await Client.ExploreFolderAsync("");
                    var tagSplit = SplitTag(tag, split);
                    var answerTag = await SearchTag(tagSplit, split, 0, rootDirectory);
                    if (answerTag != null)
                    {
                        try
                        {
                            await Client.WriteAsync(answerTag, value);
                        }
                        catch (Exception e)
                        {
                            AddInfo("opc write exception:" + e.Message);
                            return new OpcParamOut()
                            {
                                Success = false
                            };
                        }
                        return new OpcParamOut()
                        {
                            Success = true
                        };
                    }
                    return new OpcParamOut()
                    {
                        Success = false
                    };
                }
            }
            catch (Exception e)
            {
                AddInfo("opc client exception:" + e.Message);
                return new OpcParamOut()
                {
                    Success = false,
                    Value = Encoding.ASCII.GetBytes("NoData")
                };
            }
        }

        private async Task<string> SearchTag(string[] tags, char split, int deep, IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var currentTag = node.Tag.Substring(node.Tag.LastIndexOf(split) + 1);
                if (Regex.IsMatch(currentTag, tags[deep]))
                {
                    if (deep == tags.Length - 1) return node.Tag;
                    var subDirectories = await Client.ExploreFolderAsync(node.Tag);
                    var answerTag = await SearchTag(tags, split, deep + 1, subDirectories);
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
