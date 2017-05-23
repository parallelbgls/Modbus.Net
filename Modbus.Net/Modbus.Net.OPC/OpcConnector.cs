using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hylasoft.Opc.Common;
using Serilog;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc连接器
    /// </summary>
    public abstract class OpcConnector : BaseConnector<OpcParamIn, OpcParamOut>
    {
        /// <summary>
        ///     是否正在连接
        /// </summary>
        protected bool _connect;

        /// <summary>
        ///     Opc客户端
        /// </summary>
        protected IClientExtend Client;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">服务端url</param>
        protected OpcConnector(string host)
        {
            ConnectionToken = host;
        }

        /// <summary>
        ///     连接标识
        /// </summary>
        public override string ConnectionToken { get; }

        /// <summary>
        ///     是否正在连接
        /// </summary>
        public override bool IsConnected => _connect;

        /// <summary>
        ///     断开连接
        /// </summary>
        /// <returns></returns>
        public override bool Disconnect()
        {
            try
            {
                Client?.Dispose();
                Client = null;
                _connect = false;
                Log.Information("opc client {ConnectionToken} disconnected success", ConnectionToken);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "opc client {ConnectionToken} disconnected error", ConnectionToken);
                _connect = false;
                return false;
            }
        }

        /// <summary>
        ///     无返回发送数据
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public override bool SendMsgWithoutReturn(OpcParamIn message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     无返回发送数据
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public override Task<bool> SendMsgWithoutReturnAsync(OpcParamIn message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     带返回发送数据
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public override OpcParamOut SendMsg(OpcParamIn message)
        {
            return AsyncHelper.RunSync(() => SendMsgAsync(message));
        }

        /// <summary>
        ///     根据括号折叠已经打开的标签
        /// </summary>
        /// <param name="tagSplitList">已经打开的标签</param>
        /// <param name="splitChar">分割符</param>
        /// <param name="startChar">开始字符</param>
        /// <param name="endChar">结束字符</param>
        private void FoldWith(List<string> tagSplitList, char splitChar, char startChar, char endChar)
        {
            for (var i = 0; i < tagSplitList.Count; i++)
                if (tagSplitList[i].Count(ch => ch == startChar) > tagSplitList[i].Count(ch => ch == endChar))
                    for (var j = i + 1; j < tagSplitList.Count; j++)
                        if (tagSplitList[j].Contains(endChar))
                        {
                            for (var k = i + 1; k <= j; k++)
                            {
                                tagSplitList[i] += splitChar + tagSplitList[i + 1];
                                tagSplitList.RemoveAt(i + 1);
                            }
                            i--;
                            break;
                        }
        }

        /// <summary>
        ///     根据分隔符切分标签
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="split">分隔符</param>
        /// <returns>分割后的标签</returns>
        private string[] SplitTag(string tag, char split)
        {
            var tagSplitList = tag.Split(split).ToList();

            FoldWith(tagSplitList, split, '(', ')');
            FoldWith(tagSplitList, split, '[', ']');
            FoldWith(tagSplitList, split, '{', '}');

            return tagSplitList.ToArray();
        }

        /// <summary>
        ///     带返回发送数据
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
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
                        Log.Verbose($"Opc Machine {ConnectionToken} Read opc tag {answerTag} for value {result}");
                        return new OpcParamOut
                        {
                            Success = true,
                            Value = BigEndianValueHelper.Instance.GetBytes(result, result.GetType())
                        };
                    }
                    return new OpcParamOut
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
                            Log.Verbose($"Opc Machine {ConnectionToken} Write opc tag {answerTag} for value {value}");
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "opc client {ConnectionToken} write exception", ConnectionToken);
                            return new OpcParamOut
                            {
                                Success = false
                            };
                        }
                        return new OpcParamOut
                        {
                            Success = true
                        };
                    }
                    return new OpcParamOut
                    {
                        Success = false
                    };
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "opc client {ConnectionToken} read exception", ConnectionToken);
                return new OpcParamOut
                {
                    Success = false,
                    Value = Encoding.ASCII.GetBytes("NoData")
                };
            }
        }

        /// <summary>
        ///     搜索标签
        /// </summary>
        /// <param name="tags">标签</param>
        /// <param name="split">分隔符</param>
        /// <param name="deep">递归深度（第几级标签）</param>
        /// <param name="nodes">当前搜索的节点</param>
        /// <returns>搜索到的标签</returns>
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

        /// <summary>
        ///     连接PLC
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override bool Connect()
        {
            try
            {
                Client.Connect();
                _connect = true;
                Log.Information("opc client {ConnectionToken} connect success", ConnectionToken);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "opc client {ConnectionToken} connected failed", ConnectionToken);
                _connect = false;
                return false;
            }
        }

        /// <summary>
        ///     连接PLC，异步
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override Task<bool> ConnectAsync()
        {
            return Task.FromResult(Connect());
        }
    }
}