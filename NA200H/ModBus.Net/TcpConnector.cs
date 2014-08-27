using System;
using System.Net.Sockets;
using System.Threading;

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
        public delegate void ErrorShutdownEventHandler(object sender, EventArgs e);

        public delegate void ReceiveMessageHandler(object sender, SocketMessageEventArgs e);

        private readonly bool b_AsyncReceive = true;
        private readonly string host;

        // 2MB 的接收缓冲区，目的是一次接收完服务器发回的消息
        private readonly byte[] m_receiveBuffer = new byte[1024];
        private readonly int port;
        public int m_errorCount = 0;
        public int m_receiveCount = 0;
        public int m_sendCount = 0;
        private TcpClient m_socketClient;

        public TcpConnector(string ipaddress, int port, bool isAsync)
        {
            host = ipaddress;
            this.port = port;
            b_AsyncReceive = isAsync;
        }

        public bool SocketIsConnect
        {
            get { return m_socketClient != null ? m_socketClient.Connected : false; }
        }

        public void Dispose()
        {
            if (m_socketClient != null)
            {
                CloseClientSocket();
                m_socketClient.Client.Dispose();
            }
        }

        public event ReceiveMessageHandler MessageReceive;
        public event ErrorShutdownEventHandler SocketErrorShutdown;

        private void ReceiveMessage(byte[] message)
        {
            if (MessageReceive != null)
            {
                MessageReceive(this, new SocketMessageEventArgs(message));
            }
        }

        public override bool Connect()
        {
            if (m_socketClient != null)
            {
                Disconnect();
            }
            lock (this)
            {
                try
                {
                    m_socketClient = new TcpClient(host, port);
                    m_socketClient.ReceiveTimeout = 10*1000;

                    if (m_socketClient.Connected)
                    {
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
        }

        public override bool Disconnect()
        {
            lock (this)
            {
                if (m_socketClient == null)
                {
                    return true;
                }

                try
                {
                    m_socketClient.Close();
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
                    m_socketClient = null;
                }
            }
        }

        private void AddInfo(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        ///     发送数据，不需要返回任何值
        /// </summary>
        /// <param name="message">发送的信息</param>
        /// <returns>是否发送成功</returns>
        public override bool SendMsgWithoutReturn(byte[] message)
        {
            byte[] datagram = message;

            try
            {
                m_socketClient.Client.Send(datagram);

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

        /// <summary>
        ///     发送数据，需要返回
        /// </summary>
        /// <param name="message">发送的数据</param>
        /// <returns>是否发送成功</returns>
        public override byte[] SendMsg(byte[] message)
        {
            byte[] datagram = message;

            try
            {
                if (!SocketIsConnect)
                {
                    Connect();
                }
                m_socketClient.Client.Send(datagram);

                RefreshSendCount();
                //this.AddInfo("send text len = " + datagramText.Length.ToString());

                return Receive();
            }
            catch (Exception err)
            {
                AddInfo("send exception: " + err.Message);
                CloseClientSocket();
                return null;
            }
        }

        public byte[] Receive()
        {
            try
            {
                // 异步接收回答
                if (b_AsyncReceive)
                {
                    m_socketClient.Client.BeginReceive(m_receiveBuffer, 0, m_receiveBuffer.Length, SocketFlags.None,
                        EndReceiveDatagram, this);
                    return null;
                }
                    // 同步接收回答
                int len = m_socketClient.Client.Receive(m_receiveBuffer, 0, m_receiveBuffer.Length, SocketFlags.None);
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
            Array.Copy(m_receiveBuffer, replyMessage, len);

            //this.AddInfo("reply: " + replyMesage);
            ReceiveMessage(replyMessage);
            RefreshReceiveCount();

            if (len <= 0)
            {
                RefreshErrorCount();
            }

            return replyMessage;
        }

        private void RefreshSendCount()
        {
            m_sendCount++;
        }

        private void RefreshReceiveCount()
        {
            m_receiveCount++;
        }

        private void RefreshErrorCount()
        {
            m_errorCount++;
        }

        private void CloseClientSocket()
        {
            try
            {
                m_socketClient.Client.Shutdown(SocketShutdown.Both);
                m_socketClient.Client.Close();
                if (!SocketIsConnect)
                {
                    if (SocketErrorShutdown != null)
                    {
                        Thread.Sleep(1000);
                        SocketErrorShutdown(this, new EventArgs());
                        AddInfo("socket error disconnected!");
                    }
                }
            }
            catch (Exception ex)
            {
                //this.AddInfo("client close exception: " + ex.Message);
            }
        }

        /// <summary>
        ///     异步接收消息返回函数
        /// </summary>
        /// <param name="iar">异步参数</param>
        private void EndReceiveDatagram(IAsyncResult iar)
        {
            try
            {
                int readBytesLength = m_socketClient.Client.EndReceive(iar);
                if (readBytesLength == 0)
                {
                    CloseClientSocket();
                }
                else // 正常数据包
                {
                    CheckReplyDatagram(readBytesLength);
                }
            }
            catch (Exception err)
            {
                AddInfo("receive exception: " + err.Message);
                CloseClientSocket();
            }
        }
    }
}