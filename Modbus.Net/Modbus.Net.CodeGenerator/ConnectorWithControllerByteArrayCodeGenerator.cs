using Microsoft.CodeAnalysis;
using System.Text;

namespace Modbus.Net.CodeGenerator
{
    [Generator]
    public class BaseConnectorCodeGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var source = $@"
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace Modbus.Net
{{
    public abstract partial class BaseConnector : BaseConnector<byte[], byte[]>
    {{
"
+ ConnectorWithControllerByteArrayCodeContent.Code("BaseConnector")
+ $@"
    }}
}}";
            context.AddSource("BaseConnectorContent.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }
    }

    [Generator]
    public class EventHandlerConnectorCodeGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var source = $@"
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace Modbus.Net
{{
    public abstract partial class EventHandlerConnector : EventHandlerConnector<byte[], byte[]>
    {{
"
+ ConnectorWithControllerByteArrayCodeContent.Code("EventHandlerConnector")
+ $@"
    }}
}}";
            context.AddSource("EventHandlerConnectorContent.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }
    }

    public static class ConnectorWithControllerByteArrayCodeContent
    {
        public static string Code(string className)
        {
            return new StringBuilder(@"
        /// <summary>
        ///     发送锁
        /// </summary>
        protected abstract AsyncLock Lock { get; }

        /// <summary>
        ///     是否为全双工
        /// </summary>
        public bool IsFullDuplex { get; }

        /// <summary>
        ///     发送超时时间
        /// </summary>
        protected abstract int TimeoutTime { get; set; }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name=""timeoutTime"">发送超时时间</param>
        /// <param name=""isFullDuplex"">是否为全双工</param>
        protected {%0}(int timeoutTime = 10000, bool isFullDuplex = false)
        {
            IsFullDuplex = isFullDuplex;
            if (timeoutTime < -1) timeoutTime = -1;
            TimeoutTime = timeoutTime;
        }

        /// <inheritdoc />
        public override async Task<byte[]> SendMsgAsync(byte[] message)
        {
            var ans = await SendMsgInner(message);
            if (ans == null) return new byte[0];
            return ans.ReceiveMessage;
        }

        /// <summary>
        ///     发送内部
        /// </summary>
        /// <param name=""message"">发送的信息</param>
        /// <param name=""repeat"">是否为重发消息</param>
        /// <returns>发送信息的定义</returns>
        protected async Task<MessageWaitingDef> SendMsgInner(byte[] message, bool repeat = false)
        {
            IDisposable asyncLock = null;
            try
            {
                if (Controller.IsSending != true)
                {
                    Controller.SendStart();
                }
                var messageSendingdef = Controller.AddMessage(message);
                if (messageSendingdef != null)
                {
                    if (!IsFullDuplex)
                    {
                        asyncLock = await Lock.LockAsync();
                    }
                    var success = messageSendingdef.SendMutex.WaitOne(TimeoutTime);
                    if (success)
                    {
                        await SendMsgWithoutConfirm(message);
                        success = messageSendingdef.ReceiveMutex.WaitOne(TimeoutTime);
                        if (success)
                        {
                            if (!repeat && messageSendingdef.ReceiveMessage == null)
                            {
                                asyncLock?.Dispose();
                                return await SendMsgInner(message, true);
                            }
                            return messageSendingdef;
                        }
                    }
                    Controller.ForceRemoveWaitingMessage(messageSendingdef);
                }
                logger.LogInformation(""Message is waiting in {0}. Cancel!"", ConnectionToken);
                return null;
            }
            catch (Exception e)
            {
                logger.LogError(e, ""Connector {0} Send Error."", ConnectionToken);
                return null;
            }
            finally
            {
                asyncLock?.Dispose();
            }
        }").Replace("{%0}", className).ToString();
        }
    }
}
