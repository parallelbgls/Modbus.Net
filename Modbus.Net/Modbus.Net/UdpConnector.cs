using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Serilog;

namespace Modbus.Net
{
    /// <summary>
    ///     Udp收发类
    /// </summary>
    public class UdpConnector : BaseConnector, IDisposable
    {
        private readonly string _host;
        private readonly int _port;

        /// <summary>
        ///     1MB 的接收缓冲区
        /// </summary>
        private readonly byte[] _receiveBuffer = new byte[1024];

        private int _errorCount;
        private int _receiveCount;

        private int _sendCount;

        private UdpClient _socketClient;

        private bool m_disposed;

        private Task _receiveThread;
        private bool _taskCancel = false;

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ipaddress">Ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="timeoutTime">超时时间</param>
        public UdpConnector(string ipaddress, int port, int timeoutTime = 10000) : base(timeoutTime)
        {
            _host = ipaddress;
            _port = port;
        }

        /// <inheritdoc />
        public override string ConnectionToken => _host;

        /// <inheritdoc />
        protected override int TimeoutTime { get; set; }

        /// <inheritdoc />
        public override bool IsConnected => _socketClient?.Client != null && _socketClient.Client.Connected;

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
            if (!m_disposed)
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
                    Log.Debug("Udp client {ConnectionToken} Disposed", ConnectionToken);
                }
                m_disposed = true;
            }
        }

        /// <summary>
        ///     析构函数
        ///     当客户端没有显示调用Dispose()时由GC完成资源回收功能
        /// </summary>
        ~UdpConnector()
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
                    return true;                 
                }
                try
                {
                    _socketClient = new UdpClient();

                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeoutTime);
                    await Task.Run(() => _socketClient.Connect(_host, _port), cts.Token);

                    if (_socketClient.Client.Connected)
                    {
                        _taskCancel = false;
                        Controller.SendStart();
                        ReceiveMsgThreadStart();
                        Log.Information("Udp client {ConnectionToken} connected", ConnectionToken);
                        return true;
                    }

                    Log.Error("Udp client {ConnectionToken} connect failed.", ConnectionToken);
                    Dispose();
                    return false;
                }
                catch (Exception err)
                {
                    Log.Error(err, "Udp client {ConnectionToken} connect exception", ConnectionToken);
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
                Log.Information("Udp client {ConnectionToken} disconnected successfully", ConnectionToken);
                return true;
            }
            catch (Exception err)
            {
                Log.Error(err, "Udp client {ConnectionToken} disconnected exception", ConnectionToken);
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

                RefreshSendCount();

                Log.Verbose("Udp client {ConnectionToken} send text len = {Length}", ConnectionToken, datagram.Length);
                Log.Verbose($"Udp client {ConnectionToken} send: {String.Concat(datagram.Select(p => " " + p.ToString("X2")))}");
                await _socketClient.SendAsync(datagram, datagram.Length);
            }
            catch (Exception err)
            {
                Log.Error(err, "Udp client {ConnectionToken} send exception", ConnectionToken);
                CloseClientSocket();
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
                    var receive = await _socketClient.ReceiveAsync();

                    var len = receive.Buffer.Length;
                    // 异步接收回答
                    if (len > 0)
                    {
                        if (receive.Buffer.Clone() is byte[] receiveBytes)
                        {
                            Log.Verbose("Udp client {ConnectionToken} receive text len = {Length}", ConnectionToken,
                                receiveBytes.Length);
                            Log.Verbose(
                                $"Udp client {ConnectionToken} receive: {String.Concat(receiveBytes.Select(p => " " + p.ToString("X2")))}");
                            var isMessageConfirmed = Controller.ConfirmMessage(receiveBytes);
                            if (isMessageConfirmed == false)
                            {
                                //主动传输事件
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
                Log.Error(err, "Udp client {ConnectionToken} receive exception", ConnectionToken);
                //CloseClientSocket();
            }
        }

        private void RefreshSendCount()
        {
            _sendCount++;
            Log.Verbose("Udp client {ConnectionToken} send count: {SendCount}", ConnectionToken, _sendCount);
        }

        private void RefreshReceiveCount()
        {
            _receiveCount++;
            Log.Verbose("Udp client {ConnectionToken} receive count: {SendCount}", ConnectionToken, _receiveCount);
        }

        private void RefreshErrorCount()
        {
            _errorCount++;
            Log.Verbose("Udp client {ConnectionToken} error count: {ErrorCount}", ConnectionToken, _errorCount);
        }

        private void CloseClientSocket()
        {
            try
            {
                Controller.SendStop();
                Controller.Clear();
                ReceiveMsgThreadStop();
                if (_socketClient.Client.Connected)
                {
                    _socketClient?.Client.Disconnect(false);
                }
                _socketClient?.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Udp client {ConnectionToken} client close exception", ConnectionToken);
            }
            finally
            {
                _socketClient = null;
            }
        }
    }
}