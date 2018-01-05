using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
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

        /// <inheritdoc />
        protected override int TimeoutTime { get; set; }

        /// <summary>
        ///     错误次数
        /// </summary>
        private int _errorCount;
        /// <summary>
        ///     获取次数
        /// </summary>
        private int _receiveCount;
        /// <summary>
        ///     发送次数
        /// </summary>
        private int _sendCount;

        /// <summary>
        ///     获取线程
        /// </summary>
        private Task _receiveThread;
        /// <summary>
        ///     获取线程关闭
        /// </summary>
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
        /// <param name="isFullDuplex">是否为全双工</param>
        public ComConnector(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits, int timeoutTime = 10000, bool isFullDuplex = false) : base(timeoutTime, isFullDuplex)
        {
            //端口号 
            _com = com.Split(':')[0];
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
        private static Dictionary<string, IController> Controllers { get; } = new Dictionary<string, IController>()
            ;

        /// <inheritdoc />
        protected override IController Controller {
            get
            {
                if (Controllers.ContainsKey(_com))
                {
                    return Controllers[_com];
                }
                return null;
            }
            set
            {
                if (!Controllers.ContainsKey(_com))
                {
                    Controllers.Add(_com, value);
                }
            } 
        }

        /// <inheritdoc />
        protected override AsyncLock Lock => SerialPort.Lock;

        /// <summary>
        ///     连接中的连接器
        /// </summary>
        private static Dictionary<string, string> Linkers { get; } = new Dictionary<string, string>();

        /// <summary>
        ///     连接关键字(串口号:从站号)
        /// </summary>
        public override string ConnectionToken => _slave + ":" + _com;

        /// <summary>
        ///     获取当前连接器使用的串口
        /// </summary>
        private SerialPortLock SerialPort
        {
            get
            {
                if (Connectors.ContainsKey(_com))
                    return Connectors[_com];
                return null;
            }
        }

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
                        if (SerialPort.IsOpen)
                        {
                            SerialPort.Close();
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

        /// <inheritdoc />
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
                lock (Connectors)
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
                            ReadTimeout = TimeoutTime
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
                            _taskCancel = false;
                            SerialPort.Open();
                            ReceiveMsgThreadStart();
                            Controller.SendStart();
                        }
                    }
                }
                
                Log.Information("Com client {ConnectionToken} connect success", ConnectionToken);

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Com client {ConnectionToken} connect error", ConnectionToken);
                Dispose();
                return false;
            }
        }

        /// <inheritdoc />
        public override Task<bool> ConnectAsync()
        {
            return Task.FromResult(Connect());
        }

        /// <inheritdoc />
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

        #region 发送接收数据

        /// <summary>
        ///     带返回发送数据
        /// </summary>
        /// <param name="sendStr">需要发送的数据</param>
        /// <returns>是否发送成功</returns>
        public async Task<string> SendMsgAsync(string sendStr)
        {
            var myByte = sendStr.StringToByte_2();

            var returnBytes = await SendMsgAsync(myByte);

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

        /// <inheritdoc />
        public override async Task<byte[]> SendMsgAsync(byte[] message)
        {
            CheckOpen();
            return await base.SendMsgAsync(message);
        }

        /// <inheritdoc />
        protected override async Task SendMsgWithoutConfirm(byte[] message)
        {
            try
            {
                Log.Verbose("Com client {ConnectionToken} send msg length: {Length}", ConnectionToken,
                    message.Length);
                Log.Verbose(
                    $"Com client {ConnectionToken} send msg: {String.Concat(message.Select(p => " " + p.ToString("X2")))}");
                await Task.Run(() => SerialPort.Write(message, 0, message.Length));
            }
            catch (Exception err)
            {
                Log.Error(err, "Com client {ConnectionToken} send msg error", ConnectionToken);
                Dispose();
            }
            RefreshSendCount();
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
                    Thread.Sleep(100);
                    var returnBytes = ReadMsg();
                    if (returnBytes != null)
                    {
                        Log.Verbose("Com client {ConnectionToken} receive msg length: {Length}", ConnectionToken,
                            returnBytes.Length);
                        Log.Verbose(
                            $"Com client {ConnectionToken} receive msg: {String.Concat(returnBytes.Select(p => " " + p.ToString("X2")))}");

                        var isMessageConfirmed = Controller.ConfirmMessage(returnBytes);
                        foreach (var confirmed in isMessageConfirmed)
                        {
                            if (confirmed == false)
                            {
                                //主动传输事件
                            }
                        }

                        RefreshReceiveCount();
                    }                                      
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

                var i = ReadComm(out var data, 10, 5000, 1000);
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