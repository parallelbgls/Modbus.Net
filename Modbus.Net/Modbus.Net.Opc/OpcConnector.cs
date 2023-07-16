using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Opc
{
    /// <summary>
    ///     Opc连接器
    /// </summary>
    public abstract class OpcConnector : BaseConnector<OpcParamIn, OpcParamOut>
    {
        private static readonly ILogger<OpcConnector> logger = LogProvider.CreateLogger<OpcConnector>();

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
                logger.LogInformation("Opc client {ConnectionToken} disconnected success", ConnectionToken);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Opc client {ConnectionToken} disconnected error", ConnectionToken);
                _connect = false;
                return false;
            }
        }

        /// <inheritdoc />
        protected override void ReceiveMsgThreadStart()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void ReceiveMsgThreadStop()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override Task SendMsgWithoutConfirm(OpcParamIn message)
        {
            throw new NotImplementedException();
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
                    var tag = message.Tag;
                    if (tag != null)
                    {
                        var result = await Client.ReadAsync<object>(tag);
                        object resultTrans;
                        if (result.Value?.ToString() == "False")
                        {
                            resultTrans = (byte)0;
                        }
                        else if (result.Value?.ToString() == "True")
                        {
                            resultTrans = (byte)1;
                        }
                        else if (result.Value != null)
                        {
                            resultTrans = result.Value;
                        }
                        else
                        {
                            logger.LogError($"Opc Machine {ConnectionToken} Read Opc tag {tag} for value null");
                            return new OpcParamOut
                            {
                                Success = false,
                                Value = Encoding.ASCII.GetBytes("NoData")
                            };
                        }
                        logger.LogInformation($"Opc Machine {ConnectionToken} Read Opc tag {tag} for value {result.Value} {result.Value.GetType().FullName}");
                        return new OpcParamOut
                        {
                            Success = true,
                            Value = BigEndianLsbValueHelper.Instance.GetBytes(resultTrans, resultTrans.GetType())
                        };
                    }
                    logger.LogError($"Opc Machine {ConnectionToken} Read Opc tag null");
                    return new OpcParamOut
                    {
                        Success = false,
                        Value = Encoding.ASCII.GetBytes("NoData")
                    };
                }
                else
                {
                    var tag = message.Tag;
                    var value = message.SetValue;
                    ;
                    if (tag != null)
                    {
                        try
                        {
                            await Client.WriteAsync(tag, value);
                            logger.LogInformation($"Opc Machine {ConnectionToken} Write Opc tag {tag} for value {value}");
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Opc client {ConnectionToken} write exception", ConnectionToken);
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
                logger.LogError(e, "Opc client {ConnectionToken} read exception", ConnectionToken);
                Disconnect();
                return new OpcParamOut
                {
                    Success = false,
                    Value = Encoding.ASCII.GetBytes("NoData")
                };
            }
        }

        private bool Connect()
        {
            try
            {
                Client.Connect();
                _connect = true;
                logger.LogInformation("Opc client {ConnectionToken} connect success", ConnectionToken);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Opc client {ConnectionToken} connected failed", ConnectionToken);
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