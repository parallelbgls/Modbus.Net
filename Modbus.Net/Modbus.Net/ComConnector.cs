using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Serilog;

namespace Modbus.Net
{
    /// <summary>
    ///     具有发送锁的串口
    /// </summary>
    public class SerialPortLock : SerialPort
    {
        /// <summary>
        ///     发送锁
        /// </summary>
        public AsyncLock Lock { get; set; } = new AsyncLock();
    }

    /// <summary>
    ///     串口通讯类
    /// </summary>
    public class ComConnector : BaseConnector, IDisposable
    {
        /// <summary>
        ///     波特率
        /// </summary>
        private readonly int _baudRate;

        /// <summary>
        ///     串口地址
        /// </summary>
        private readonly string _com;

        /// <summary>
        ///     数据位
        /// </summary>
        private readonly int _dataBits;

        /// <summary>
        ///     奇偶校验
        /// </summary>
        private readonly Parity _parity;

        /// <summary>
        ///     从站号
        /// </summary>
        private readonly string _slave;

        /// <summary>
        ///     停止位
        /// </summary>
        private readonly StopBits _stopBits;

        /// <summary>
        ///     超时时间
        /// </summary>
        private readonly int _timeoutTime;

        private int _errorCount;
        private int _receiveCount;

        private int _sendCount;

        private Task _receiveThread;
        private bool _taskCancel = false;

        /// <summary>
        ///     Dispose是否执行
        /// </summary>
        private bool m_disposed;

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="com">串口地址:从站号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="timeoutTime">超时时间</param>
        public ComConnector(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits, int timeoutTime)
        {
            //端口号 
            _com = com.Split(':')[0];
            //读超时
            _timeoutTime = timeoutTime;
            //波特率
            _baudRate = baudRate;
            //奇偶校验
            _parity = parity;
            //停止位 
            _stopBits = stopBits;
            //数据位
            _dataBits = dataBits;
            //从站号
            _slave = com.Split(':')[1];
        }

        /// <summary>
        ///     连接中的串口
        /// </summary>
        private static Dictionary<string, SerialPortLock> Connectors { get; } = new Dictionary<string, SerialPortLock>()
            ;

        /// <summary>
        ///     连接中的连接器
        /// </summary>
        private static Dictionary<string, string> Linkers { get; } = new Dictionary<string, string>();

        /// <summary>
        ///     连接关键字(串口号:从站号)
        /// </summary>
        public override string ConnectionToken => _slave + ":" + _com;

        private SerialPortLock SerialPort
        {
            get
            {
                if (Connectors.ContainsKey(_com))
                    return Connectors[_com];
                return null;
            }
        }

        /// <summary>
        ///     实现IDisposable接口
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            //.NET Framework 类库
            // GC..::.SuppressFinalize 方法
            //请求系统不要调用指定对象的终结器。
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     串口读(非阻塞方式读串口，直到串口缓冲区中没有数据
        /// </summary>
        /// <param name="readBuf">串口数据缓冲 </param>
        /// <param name="bufRoom">串口数据缓冲空间大小 </param>
        /// <param name="howTime">设置串口读放弃时间 </param>
        /// <param name="byteTime">字节间隔最大时间 </param>
        /// <returns>串口实际读入数据个数 </returns>
        public int ReadComm(out byte[] readBuf, int bufRoom, int howTime, int byteTime)
        {
            readBuf = new byte[1023];
            Array.Clear(readBuf, 0, readBuf.Length);

            if (SerialPort.IsOpen == false)
                return -1;
            var nBytelen = 0;
            SerialPort.ReadTimeout = howTime;

            while (SerialPort.BytesToRead > 0)
            {
                readBuf[nBytelen] = (byte) SerialPort.ReadByte();
                var bTmp = new byte[bufRoom];
                Array.Clear(bTmp, 0, bTmp.Length);

                var nReadLen = ReadBlock(bTmp, bufRoom, byteTime);

                if (nReadLen > 0)
                {
                    Array.Copy(bTmp, 0, readBuf, nBytelen + 1, nReadLen);
                    nBytelen += 1 + nReadLen;
                }

                else if (nReadLen == 0)
                {
                    nBytelen += 1;
                }
            }

            return nBytelen;
        }

        /// <summary>
        ///     串口同步读(阻塞方式读串口，直到串口缓冲区中没有数据,靠字符间间隔超时确定没有数据)
        /// </summary>
        /// <param name="readBuf">串口数据缓冲 </param>
        /// <param name="readRoom">串口数据缓冲空间大小 </param>
        /// <param name="byteTime">字节间隔最大时间 </param>
        /// <returns>从串口实际读入的字节个数 </returns>
        public int ReadBlock(byte[] readBuf, int readRoom, int byteTime)
        {
            if (SerialPort.IsOpen == false)
                return 0;
            sbyte nBytelen = 0;
            SerialPort.ReadTimeout = byteTime;

            while (nBytelen < readRoom - 1 && SerialPort.BytesToRead > 0)
            {
                readBuf[nBytelen] = (byte) SerialPort.ReadByte();
                nBytelen++; // add one 
            }
            readBuf[nBytelen] = 0x00;
            return nBytelen;
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
                    Controller.SendStop();
                }
                // Release unmanaged resources
                if (SerialPort != null)
                {
                    if (Linkers.Values.Count(p => p == _com) <= 1)
                    {
                        try
                        {
                            SerialPort.Close();
                        }
                        catch
                        {
                            //ignore
                        }
                        SerialPort.Dispose();
                        Log.Information("Com interface {Com} Disposed", _com);
                        Connectors[_com] = null;
                        Connectors.Remove(_com);
                        ReceiveMsgThreadStop();
                    }
                    Linkers.Remove(_slave);
                    Log.Information("Com connector {ConnectionToken} Removed", ConnectionToken);
                }                
                m_disposed = true;
            }
        }

        /// <summary>
        ///     析构函数
        ///     当客户端没有显示调用Dispose()时由GC完成资源回收功能
        /// </summary>
        ~ComConnector()
        {
            Dispose(false);
        }

        private void RefreshSendCount()
        {
            _sendCount++;
            Log.Verbose("Com client {ConnectionToken} send count: {SendCount}", ConnectionToken, _sendCount);
        }

        private void RefreshReceiveCount()
        {
            _receiveCount++;
            Log.Verbose("Com client {ConnectionToken} receive count: {SendCount}", ConnectionToken, _receiveCount);
        }

        private void RefreshErrorCount()
        {
            _errorCount++;
            Log.Verbose("Com client {ConnectionToken} error count: {ErrorCount}", ConnectionToken, _errorCount);
        }

        #region 发送接收数据

        /// <summary>
        ///     是否正在连接
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                if (SerialPort != null && !SerialPort.IsOpen)
                    SerialPort.Dispose();
                return SerialPort != null && SerialPort.IsOpen && Linkers.ContainsKey(_slave);
            }
        }

        /// <summary>
        ///     连接串口
        /// </summary>
        /// <returns>是否连接成功</returns>
        protected bool Connect()
        {
            try
            {
                if (!Connectors.ContainsKey(_com))
                {
                    Connectors.Add(_com, new SerialPortLock
                    {
                        PortName = _com,
                        BaudRate = _baudRate,
                        Parity = _parity,
                        StopBits = _stopBits,
                        DataBits = _dataBits,
                        ReadTimeout = _timeoutTime
                    });
                }
                if (!Linkers.ContainsKey(_slave))
                {
                    Linkers.Add(_slave, _com);
                }
                if (!SerialPort.IsOpen)
                {
                    lock (SerialPort)
                    {
                        SerialPort.Open();
                        ReceiveMsgThreadStart();
                    }
                }

                Controller.SendStart();

                Log.Information("Com client {ConnectionToken} connect success", ConnectionToken);

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Com client {ConnectionToken} connect error", ConnectionToken);
                return false;
            }
        }

        /// <summary>
        ///     连接串口
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override Task<bool> ConnectAsync()
        {
            return Task.FromResult(Connect());
        }

        /// <summary>
        ///     断开串口
        /// </summary>
        /// <returns>是否断开成功</returns>
        public override bool Disconnect()
        {
            if (Linkers.ContainsKey(_slave) && Connectors.ContainsKey(_com))
                try
                {
                    Dispose();
                    Log.Information("Com client {ConnectionToken} disconnect success", ConnectionToken);
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Com client {ConnectionToken} disconnect error", ConnectionToken);
                    return false;
                }
            Log.Error(new Exception("Linkers or Connectors Dictionary not found"),
                "Com client {ConnectionToken} disconnect error", ConnectionToken);
            return false;
        }

        /// <summary>
        ///     带返回发送数据
        /// </summary>
        /// <param name="sendStr">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public string SendMsg(string sendStr)
        {
            var myByte = sendStr.StringToByte_2();

            var returnBytes = SendMsg(myByte);

            return returnBytes.ByteToString();
        }

        /// <summary>
        ///     确认串口是否已经打开，如果没有打开则尝试打开两次，连续失败直接释放连接资源
        /// </summary>
        protected void CheckOpen()
        {
            if (!SerialPort.IsOpen)
            {
                try
                {
                    SerialPort.Open();
                }
                catch (Exception err)
                {
                    Log.Error(err, "Com client {ConnectionToken} open error", ConnectionToken);
                    Dispose();
                    try
                    {
                        SerialPort.Open();
                    }
                    catch (Exception err2)
                    {
                        Log.Error(err2, "Com client {ConnectionToken} open error", ConnectionToken);
                        Dispose();
                    }
                }
            }
        }

        /// <summary>
        ///     发送数据，需要返回
        /// </summary>
        /// <param name="message">发送的数据</param>
        /// <returns>是否发送成功</returns>
        protected byte[] SendMsg(byte[] message)
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
            CheckOpen();
            var task = SendMsgInner(message).WithCancellation(new CancellationTokenSource(10000).Token);
            var ans = await task;
            if (task.IsCanceled)
            {
                Controller.ForceRemoveWaitingMessage(ans);
                return null;
            }
            return ans.ReceiveMessage;
        }

        private async Task<MessageWaitingDef> SendMsgInner(byte[] message)
        {
            var messageSendingdef = Controller.AddMessage(message);
            messageSendingdef.SendMutex.WaitOne();
            await SendMsgWithoutConfirm(message);
            messageSendingdef.ReceiveMutex.WaitOne();
            return messageSendingdef;
        }

        /// <summary>
        ///     发送数据，不确认
        /// </summary>
        /// <param name="message">需要发送的数据</param>
        protected override async Task SendMsgWithoutConfirm(byte[] message)
        {
            using (await SerialPort.Lock.LockAsync())
            {
                try
                {
                    Log.Verbose("Com client {ConnectionToken} send msg length: {Length}", ConnectionToken,
                        message.Length);
                    Log.Verbose(
                        $"Com client {ConnectionToken} send msg: {String.Concat(message.Select(p => " " + p.ToString("X2")))}");
                    await Task.Run(()=>SerialPort.Write(message, 0, message.Length));
                }
                catch (Exception err)
                {
                    Log.Error(err, "Com client {ConnectionToken} send msg error", ConnectionToken);
                }
                RefreshSendCount();
            }
        }

        /// <inheritdoc />
        protected override void ReceiveMsgThreadStart()
        {
            _receiveThread = Task.Run(()=>ReceiveMessage());
        }

        /// <inheritdoc />
        protected override void ReceiveMsgThreadStop()
        {
            _taskCancel = true;
        }

        private void ReceiveMessage()
        {
            while (!_taskCancel)
            {
                try
                {
                    var returnBytes = ReadMsg();
                    if (returnBytes != null)
                    {
                        Log.Verbose("Com client {ConnectionToken} receive msg length: {Length}", ConnectionToken,
                            returnBytes.Length);
                        Log.Verbose(
                            $"Com client {ConnectionToken} receive msg: {String.Concat(returnBytes.Select(p => " " + p.ToString("X2")))}");

                        var isMessageConfirmed = Controller.ConfirmMessage(returnBytes);
                        if (isMessageConfirmed == false)
                        {
                            //主动传输事件
                        }
                    }
                    RefreshReceiveCount();

                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Com client {ConnectionToken} read msg error", ConnectionToken);
                }
            }
        }

        private byte[] ReadMsg()
        {
            try
            {
                CheckOpen();

                byte[] data;
                Thread.Sleep(100);
                var i = ReadComm(out data, 10, 5000, 1000);
                if (i > 0)
                {
                    var returndata = new byte[i];
                    Array.Copy(data, 0, returndata, 0, i);
                    return returndata;
                }
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e, "Com client {ConnectionToken} read error", ConnectionToken);
                RefreshErrorCount();
                Dispose();
                return null;
            }
        }

        #endregion
    }
}