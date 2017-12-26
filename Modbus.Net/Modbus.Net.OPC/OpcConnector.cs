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
    public abstract class OpcConnector : IConnector<OpcParamIn, OpcParamOut>
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
        ///     是否开启正则匹配
        /// </summary>
        protected bool RegexOn { get; set; }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="host">服务端url</param>
        /// <param name="isRegexOn">是否开启正则匹配</param>
        protected OpcConnector(string host, bool isRegexOn)
        {
            ConnectionToken = host;
            RegexOn = isRegexOn;
        }

        /// <summary>
        ///     连接标识
        /// </summary>
        public virtual string ConnectionToken { get; }

        /// <summary>
        ///     是否正在连接
        /// </summary>
        public virtual bool IsConnected => _connect;

        /// <summary>
        ///     断开连接
        /// </summary>
        /// <returns></returns>
        public virtual bool Disconnect()
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
        ///     带返回发送数据
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public virtual async Task<OpcParamOut> SendMsgAsync(OpcParamIn message)
        {
            try
            {
                if (message.IsRead)
                {
                    var split = message.Split;
                    var tag = message.Tag;
                    var rootDirectory = await Client.ExploreFolderAsync("");
                    var answerTag = await SearchTag(tag, split, 0, rootDirectory);
                    if (answerTag != null)
                    {
                        var result = await Client.ReadAsync<object>(answerTag);
                        Log.Verbose($"Opc Machine {ConnectionToken} Read opc tag {answerTag} for value {result.Value}");
                        return new OpcParamOut
                        {
                            Success = true,
                            Value = BigEndianValueHelper.Instance.GetBytes(result.Value, result.Value.GetType())
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
                    var answerTag = await SearchTag(tag, split, 0, rootDirectory);
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
                if (RegexOn && Regex.IsMatch(currentTag, tags[deep]) || !RegexOn && currentTag == tags[deep])
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
        protected bool Connect()
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
        public virtual Task<bool> ConnectAsync()
        {
            return Task.FromResult(Connect());
        }
    }
}