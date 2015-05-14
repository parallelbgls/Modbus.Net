using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ModBus.Net
{
    /// <summary>
    ///     Socket收到的数据
    /// </summary>
    public class SocketMessageEventArgs : EventArgs
    {
        public SocketMessageEventArgs(byte[] message)
        {
            Message = message;
        }

        public byte[] Message { get; set; }
    }

    /// <summary>
    ///     Socket收发类
    ///     作者：本类来源于CSDN，并由罗圣（Chris L.）根据实际需要修改
    /// </summary>
    public class TcpConnector : BaseConnector, IDisposable
    {
        public override string ConnectionToken { get { return _host; } }

        private readonly string _host;

        private int _sendCount;
        private int _receiveCount;
        private int _errorCount;
        private NetworkStream _stream;

        // 2MB 的接收缓冲区，目的是一次接收完服务器发回的消息
        private readonly byte[] _receiveBuffer = new byte[1024];
        private readonly int _port;
        private TcpClient _socketClient;

        private int _timeoutTime;

        public int TimeoutTime
        {
            get { return _timeoutTime; }
            set
            {
                _timeoutTime = value;
                if (_socketClient != null)
                {
                    _socketClient.ReceiveTimeout = _timeoutTime;
                }
            }
        }

        public TcpConnector(string ipaddress, int port, int timeoutTime)
        {
            _host = ipaddress;
            _port = port;
            TimeoutTime = timeoutTime;
        }

        public override bool IsConnected
        {
            get { return _socketClient != null && _socketClient.Connected; }
        }

        public void Dispose()
        {
            if (_socketClient != null)
            {
                CloseClientSocket();
                _socketClient.Client.Dispose();
            }
        }

        public override bool Connect()
        {
            return AsyncHelper.RunSync(ConnectAsync);
        }

        public override async Task<bool> ConnectAsync()
        {
            if (_socketClient != null)
            {
                Disconnect();
            }
                try
                {
                    _socketClient = new TcpClient
                    {
                        ReceiveTimeout = TimeoutTime                      
                    };

                    try
                    {
                        await _socketClient.ConnectAsync(_host, _port);
                    }
                    catch (Exception e)
                    {
                        AddInfo("client connected exception: " + e.Message);
                    }
                    if (_socketClient.Connected)
                    {
                        _stream = _socketClient.GetStream();
                        AddInfo("client connected.");
                        return true;
                    }
                    AddInfo("connect failed.");
                    return false;
                }
                catch (Exception err)
                {
                    AddInfo("client connect exception: " + err.Message);
                    return false;
                }
        }

        public override bool Disconnect()
        {
            if (_socketClient == null)
            {
                return true;
            }

            try
            {
                _socketClient.Close();
                AddInfo("client disconnected successfully.");
                return true;
            }
            catch (Exception err)
            {
                AddInfo("client disconnected exception: " + err.Message);
                return false;
            }
            finally
            {
                _socketClient = null;
            }
        }

        private void AddInfo(string message)
        {
            Console.WriteLine(message);
        }

        public override bool SendMsgWithoutReturn(byte[] message)
        {
            return AsyncHelper.RunSync(() => SendMsgWithoutReturnAsync(message));
        }

        /// <summary>
        ///     发送数据，不需要返回任何值
        /// </summary>
        /// <param name="message">发送的信息</param>
        /// <returns>是否发送成功</returns>
        public override async Task<bool> SendMsgWithoutReturnAsync(byte[] message)
        {
            byte[] datagram = message;

            try
            {
                if (!IsConnected)
                {
                    await ConnectAsync();
                }
                
                await _stream.WriteAsync(datagram, 0, datagram.Length);

                RefreshSendCount();
                //this.AddInfo("send text len = " + datagramText.Length.ToString());
                return true;
            }
            catch (Exception err)
            {
                AddInfo("send exception: " + err.Message);
                CloseClientSocket();
                return false;
            }
        }

        public override byte[] SendMsg(byte[] message)
        {
            return AsyncHelper.RunSync(() => SendMsgAsync(message));
        }

        /// <summary>
        ///     发送数据，需要返回
        /// </summary>
        /// <param name="message">发送的数据</param>
        /// <returns>是否发送成功</returns>
        public override async Task<byte[]> SendMsgAsync(byte[] message)
        {
            byte[] datagram = message;

            try
            {
                if (!IsConnected)
                {
                    await ConnectAsync();
                }

                var stream = _socketClient.GetStream();
                await stream.WriteAsync(datagram, 0, datagram.Length);

                RefreshSendCount();
                //this.AddInfo("send text len = " + datagramText.Length.ToString());

                return await ReceiveAsync(stream);
            }
            catch (Exception err)
            {
                AddInfo("send exception: " + err.Message);
                CloseClientSocket();
                return null;
            }
        }

        protected async Task<byte[]> ReceiveAsync(NetworkStream stream)
        {
            try
            {
                int len = await stream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length);
                // 异步接收回答
                if (len > 0)
                {
                    return CheckReplyDatagram(len);
                }
                return null;
            }
            catch (Exception err)
            {
                AddInfo("receive exception: " + err.Message);
                CloseClientSocket();
                return null;
            }
        }

        /// <summary>
        ///     接收消息，并转换成字符串
        /// </summary>
        /// <param name="len"></param>
        private byte[] CheckReplyDatagram(int len)
        {
            var replyMessage = new byte[len];
            Array.Copy(_receiveBuffer, replyMessage, len);

            //this.AddInfo("reply: " + replyMesage);
            RefreshReceiveCount();

            if (len <= 0)
            {
                RefreshErrorCount();
            }

            return replyMessage;
        }

        private void RefreshSendCount()
        {
            _sendCount++;
        }

        private void RefreshReceiveCount()
        {
            _receiveCount++;
        }

        private void RefreshErrorCount()
        {
            _errorCount++;
        }

        private void CloseClientSocket()
        {
            try
            {
                _stream.Close();
                _socketClient.Client.Shutdown(SocketShutdown.Both);
                _socketClient.Client.Close();
            }
            catch (Exception ex)
            {
                AddInfo("client close exception: " + ex.Message);
            }
        }
    }
}