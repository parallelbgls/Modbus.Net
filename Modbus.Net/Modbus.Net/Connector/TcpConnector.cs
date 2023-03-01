using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     Socket收发类
    ///     作者：本类来源于CSDN，并由罗圣（Chris L.）根据实际需要修改
    /// </summary>
    public class TcpConnector : BaseConnector, IDisposable
    {
        private static readonly ILogger<TcpConnector> logger = LogProvider.CreateLogger<TcpConnector>();

        private readonly string _host;
        private readonly int _port;

        /// <summary>
        ///     1MB 的接收缓冲区
        /// </summary>
        private readonly byte[] _receiveBuffer = new byte[1024];

        private int _errorCount;
        private int _receiveCount;

        private int _sendCount;

        private TcpClient _socketClient;

        private int _timeoutTime;

        private Task _receiveThread;
        private bool _taskCancel = false;

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
        protected override int TimeoutTime
        {
            get =>
            _timeoutTime;
            set
            {
                _timeoutTime = value;
                if (_socketClient != null)
                    _socketClient.ReceiveTimeout = _timeoutTime;
            }
        }

        /// <inheritdoc />
        public override bool IsConnected => _socketClient?.Client != null && _socketClient.Connected;

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
            if (_socketClient != null)
            {
                CloseClientSocket();
                _socketClient = null;
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
                if (_socketClient != null)
                {
                    if (_socketClient.Connected)
                        return true;
                }
                try
                {
                    _socketClient = new TcpClient
                    {
                        SendTimeout = TimeoutTime,
                        ReceiveTimeout = TimeoutTime
                    };

                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeoutTime);
                    await _socketClient.ConnectAsync(_host, _port).WithCancellation(cts.Token);

                    if (_socketClient.Connected)
                    {
                        _taskCancel = false;
                        Controller.SendStart();
                        ReceiveMsgThreadStart();
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
                    Dispose();
                    return false;
                }
            }
        }

        /// <inheritdoc />
        public override bool Disconnect()
        {
            if (_socketClient == null)
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
                return false;
            }
            finally
            {
                _socketClient = null;
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

                var stream = _socketClient.GetStream();

                RefreshSendCount();

                logger.LogDebug("Tcp client {ConnectionToken} send text len = {Length}", ConnectionToken, datagram.Length);
                logger.LogDebug($"Tcp client {ConnectionToken} send: {String.Concat(datagram.Select(p => " " + p.ToString("X2")))}");
                await stream.WriteAsync(datagram, 0, datagram.Length);
            }
            catch (Exception err)
            {
                logger.LogError(err, "Tcp client {ConnectionToken} send exception", ConnectionToken);
                Dispose();
            }
        }

        /// <inheritdoc />
        protected override void ReceiveMsgThreadStart()
        {
            _receiveThread = Task.Run(ReceiveMessage);
        }

        /// <inheritdoc />
        protected override void ReceiveMsgThreadStop()
        {
            _taskCancel = true;
        }

        /// <summary>
        ///     接收返回消息
        /// </summary>
        /// <returns>返回的消息</returns>
        protected async Task ReceiveMessage()
        {
            try
            {
                while (!_taskCancel)
                {
                    if (_socketClient == null) break;
                    NetworkStream stream = _socketClient.GetStream();
                    var len = await stream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length);
                    stream.Flush();

                    // 异步接收回答
                    if (len > 0)
                    {
                        byte[] receiveBytes = CheckReplyDatagram(len);
                        logger.LogDebug("Tcp client {ConnectionToken} receive text len = {Length}", ConnectionToken,
                            receiveBytes.Length);
                        logger.LogDebug(
                            $"Tcp client {ConnectionToken} receive: {String.Concat(receiveBytes.Select(p => " " + p.ToString("X2")))}");
                        var isMessageConfirmed = Controller.ConfirmMessage(receiveBytes);
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

                        RefreshReceiveCount();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                //ignore
            }
            catch (Exception err)
            {
                logger.LogError(err, "Tcp client {ConnectionToken} receive exception", ConnectionToken);
                //CloseClientSocket();
            }
        }

        /// <summary>
        ///     接收消息，并转换成字符串
        /// </summary>
        /// <param name="len">消息的长度</param>
        private byte[] CheckReplyDatagram(int len)
        {
            var replyMessage = new byte[len];
            Array.Copy(_receiveBuffer, replyMessage, len);

            if (len <= 0)
                RefreshErrorCount();

            return replyMessage;
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

        private void CloseClientSocket()
        {
            try
            {
                Controller.SendStop();
                Controller.Clear();
                ReceiveMsgThreadStop();
                if (_socketClient != null)
                {
                    if (_socketClient.Connected)
                    {
                        _socketClient.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Tcp client {ConnectionToken} client close exception", ConnectionToken);
            }
        }
    }
}