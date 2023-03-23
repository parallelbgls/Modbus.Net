using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     Socket收发类
    ///     作者：本类来源于CSDN，并由罗圣（Chris L.）根据实际需要修改
    /// </summary>
    public class TcpConnector : EventHandlerConnector, IDisposable
    {
        private static readonly ILogger<TcpConnector> logger = LogProvider.CreateLogger<TcpConnector>();

        private readonly string _host;
        private readonly int _port;

        private int _errorCount;
        private int _receiveCount;

        private int _sendCount;

        private IChannel Channel { get; set; }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ipaddress">Ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="timeoutTime">超时时间</param>
        /// <param name="isFullDuplex">是否为全双工</param>
        public TcpConnector(string ipaddress, int port, int timeoutTime = 10000, bool isFullDuplex = true) : base(timeoutTime, isFullDuplex)
        {
            _host = ipaddress;
            _port = port;
        }

        /// <inheritdoc />
        public override string ConnectionToken => _host;

        /// <inheritdoc />
        protected override int TimeoutTime { get; set; }

        /// <inheritdoc />
        public override bool IsConnected => Channel?.Open == true;

        /// <inheritdoc />
        protected override AsyncLock Lock { get; } = new AsyncLock();

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            //.NET Framework 类库
            // GC..::.SuppressFinalize 方法
            //请求系统不要调用指定对象的终结器。
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     虚方法，可供子类重写
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
            }
            // Release unmanaged resources
            if (Channel != null)
            {
                CloseClientSocket().Wait();
                Channel = null;
                logger.LogDebug("Tcp client {ConnectionToken} Disposed", ConnectionToken);
            }
        }

        /// <summary>
        ///     析构函数
        ///     当客户端没有显示调用Dispose()时由GC完成资源回收功能
        /// </summary>
        ~TcpConnector()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public override async Task<bool> ConnectAsync()
        {
            using (await Lock.LockAsync())
            {
                if (Channel != null)
                {
                    if (Channel.Open)
                        return true;
                }
                try
                {
                    var bootstrap = new Bootstrap();
                    bootstrap
                        .Group(new MultithreadEventLoopGroup())
                        .Channel<TcpSocketChannel>()
                        .Option(ChannelOption.TcpNodelay, true)
                        .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(TimeoutTime))
                        .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                        {
                            IChannelPipeline pipeline = channel.Pipeline;

                            pipeline.AddLast("handler", this);
                        }));

                    Channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_host), _port));

                    if (Channel.Open)
                    {
                        Controller.SendStart();
                        logger.LogInformation("Tcp client {ConnectionToken} connected", ConnectionToken);
                        return true;
                    }
                    logger.LogError("Tcp client {ConnectionToken} connect failed.", ConnectionToken);
                    Dispose();
                    return false;
                }
                catch (Exception err)
                {
                    logger.LogError(err, "Tcp client {ConnectionToken} connect exception", ConnectionToken);

                    RefreshErrorCount();

                    Dispose();
                    return false;
                }
            }
        }

        /// <inheritdoc />
        public override bool Disconnect()
        {
            if (Channel.Open)
                return true;

            try
            {
                Dispose();
                logger.LogInformation("Tcp client {ConnectionToken} disconnected successfully", ConnectionToken);
                return true;
            }
            catch (Exception err)
            {
                logger.LogError(err, "Tcp client {ConnectionToken} disconnected exception", ConnectionToken);

                RefreshErrorCount();

                return false;
            }
            finally
            {
                Channel = null;
            }
        }

        /// <inheritdoc />
        protected override async Task SendMsgWithoutConfirm(byte[] message)
        {
            var datagram = message;

            try
            {
                if (!IsConnected)
                    await ConnectAsync();

                RefreshSendCount();

                logger.LogDebug("Tcp client {ConnectionToken} send text len = {Length}", ConnectionToken, datagram.Length);
                logger.LogDebug($"Tcp client {ConnectionToken} send: {string.Concat(datagram.Select(p => " " + p.ToString("X2")))}");
                IByteBuffer buffer = Unpooled.Buffer();
                buffer.WriteBytes(datagram);
                await Channel.WriteAndFlushAsync(buffer);
            }
            catch (Exception err)
            {
                logger.LogError(err, "Tcp client {ConnectionToken} send exception", ConnectionToken);

                RefreshErrorCount();

                Dispose();
            }
        }

        /// <inheridoc />
        public override async void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is IByteBuffer buffer)
                {
                    byte[] msg = buffer.Array.Slice(buffer.ArrayOffset, buffer.ReadableBytes);
                    logger.LogDebug("Tcp client {ConnectionToken} receive text len = {Length}", ConnectionToken,
                       msg.Length);
                    logger.LogDebug(
                        $"Tcp client {ConnectionToken} receive: {string.Concat(msg.Select(p => " " + p.ToString("X2")))}");
                    var isMessageConfirmed = Controller.ConfirmMessage(msg);
                    if (isMessageConfirmed != null)
                    {
                        foreach (var confirmed in isMessageConfirmed)
                        {
                            if (confirmed.Item2 == false)
                            {
                                var sendMessage = InvokeReturnMessage(confirmed.Item1);
                                //主动传输事件
                                if (sendMessage != null)
                                {
                                    await SendMsgWithoutConfirm(sendMessage);
                                }
                            }
                        }
                    }

                    RefreshReceiveCount();
                }
            }
            catch (ObjectDisposedException)
            {
                //ignore
            }
            catch (Exception err)
            {
                logger.LogError(err, "Tcp client {ConnectionToken} receive exception", ConnectionToken);

                RefreshErrorCount();

                await CloseClientSocket();
            }
        }

        private void RefreshSendCount()
        {
            _sendCount++;
            logger.LogDebug("Tcp client {ConnectionToken} send count: {SendCount}", ConnectionToken, _sendCount);
        }

        private void RefreshReceiveCount()
        {
            _receiveCount++;
            logger.LogDebug("Tcp client {ConnectionToken} receive count: {SendCount}", ConnectionToken, _receiveCount);
        }

        private void RefreshErrorCount()
        {
            _errorCount++;
            logger.LogDebug("Tcp client {ConnectionToken} error count: {ErrorCount}", ConnectionToken, _errorCount);
        }

        private async Task CloseClientSocket()
        {
            try
            {
                Controller.SendStop();
                Controller.Clear();
                if (Channel != null)
                {
                    if (Channel.Open)
                    {
                        await Channel.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Tcp client {ConnectionToken} client close exception", ConnectionToken);

                RefreshErrorCount();
            }
        }
    }
}