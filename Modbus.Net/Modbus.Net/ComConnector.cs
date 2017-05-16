using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public class SerialPortLock : SerialPort
    {
        public object Lock { get; set; } = new object();
    }

    /// <summary>
    ///     串口通讯类
    /// </summary>
    public class ComConnector : BaseConnector, IDisposable
    {
        public delegate byte[] GetDate(byte[] bts);

        private static Dictionary<string, SerialPortLock> Connectors { get; } = new Dictionary<string, SerialPortLock>();
        private static Dictionary<string, string> Linkers { get; } = new Dictionary<string, string>();

        private readonly int _baudRate;

        //private GetDate mygetDate;
        private readonly string _com;
        private readonly int _dataBits;
        private readonly Parity _parity;
        private readonly StopBits _stopBits;
        private readonly int _timeoutTime;
        private readonly string _slave;

        private bool m_disposed = false;

        public ComConnector(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits, int timeoutTime)
        {
            _com = com.Split(':')[0];
            _timeoutTime = timeoutTime;
            _baudRate = baudRate;
            _parity = parity;
            _stopBits = stopBits;
            _dataBits = dataBits;
            _slave = com.Split(':')[1];

            //端口号 
            //读超时
            //比特率 
            //奇偶校验 
            //停止位 
            //数据位
            //从站号标识
        }

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
        /// <param name="HowTime">设置串口读放弃时间 </param>
        /// <param name="ByteTime">字节间隔最大时间 </param>
        /// <returns>串口实际读入数据个数 </returns>
        public int ReadComm(out byte[] readBuf, int bufRoom, int HowTime, int ByteTime)
        {
            //throw new System.NotImplementedException(); 
            readBuf = new byte[1023];
            Array.Clear(readBuf, 0, readBuf.Length);

            int nReadLen, nBytelen;
            if (SerialPort.IsOpen == false)
                return -1;
            nBytelen = 0;
            SerialPort.ReadTimeout = HowTime;


            try
            {
                while (SerialPort.BytesToRead > 0)
                {
                    readBuf[nBytelen] = (byte) SerialPort.ReadByte();
                    var bTmp = new byte[bufRoom];
                    Array.Clear(bTmp, 0, bTmp.Length);

                    nReadLen = ReadBlock(bTmp, bufRoom, ByteTime);

                    if (nReadLen > 0)
                    {
                        Array.Copy(bTmp, 0, readBuf, nBytelen + 1, nReadLen);
                        nBytelen += 1 + nReadLen;
                    }

                    else if (nReadLen == 0)
                        nBytelen += 1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return nBytelen;
        }

        /// <summary>
        ///     串口同步读(阻塞方式读串口，直到串口缓冲区中没有数据,靠字符间间隔超时确定没有数据)
        /// </summary>
        /// <param name="ReadBuf">串口数据缓冲 </param>
        /// <param name="ReadRoom">串口数据缓冲空间大小 </param>
        /// <param name="ByteTime">字节间隔最大时间 </param>
        /// <returns>从串口实际读入的字节个数 </returns>
        public int ReadBlock(byte[] ReadBuf, int ReadRoom, int ByteTime)
        {
            sbyte nBytelen;
            //long nByteRead; 

            if (SerialPort.IsOpen == false)
                return 0;
            nBytelen = 0;
            SerialPort.ReadTimeout = ByteTime;

            while (nBytelen < ReadRoom - 1 && SerialPort.BytesToRead > 0)
            {
                try
                {
                    ReadBuf[nBytelen] = (byte) SerialPort.ReadByte();
                    nBytelen++; // add one 
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            ReadBuf[nBytelen] = 0x00;
            return nBytelen;
        }


        /// <summary>
        ///     字符数组转字符串16进制
        /// </summary>
        /// <param name="InBytes"> 二进制字节 </param>
        /// <returns>类似"01 02 0F" </returns>
        public static string ByteToString(byte[] InBytes)
        {
            var StringOut = "";
            foreach (var InByte in InBytes)
            {
                StringOut = StringOut + string.Format("{0:X2}", InByte) + " ";
            }

            return StringOut.Trim();
        }

        /// <summary>
        ///     strhex 转字节数组
        /// </summary>
        /// <param name="InString">类似"01 02 0F" 用空格分开的  </param>
        /// <returns> </returns>
        public static byte[] StringToByte(string InString)
        {
            string[] ByteStrings;
            ByteStrings = InString.Split(" ".ToCharArray());
            byte[] ByteOut;
            ByteOut = new byte[ByteStrings.Length];
            for (var i = 0; i <= ByteStrings.Length - 1; i++)
            {
                ByteOut[i] = byte.Parse(ByteStrings[i], NumberStyles.HexNumber);
            }
            return ByteOut;
        }

        /// <summary>
        ///     strhex 转字节数组
        /// </summary>
        /// <param name="InString">类似"01 02 0F" 中间无空格 </param>
        /// <returns> </returns>
        public static byte[] StringToByte_2(string InString)
        {
            byte[] ByteOut;
            InString = InString.Replace(" ", "");
            try
            {
                var ByteStrings = new string[InString.Length/2];
                var j = 0;
                for (var i = 0; i < ByteStrings.Length; i++)
                {
                    ByteStrings[i] = InString.Substring(j, 2);
                    j += 2;
                }

                ByteOut = new byte[ByteStrings.Length];
                for (var i = 0; i <= ByteStrings.Length - 1; i++)
                {
                    ByteOut[i] = byte.Parse(ByteStrings[i], NumberStyles.HexNumber);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return ByteOut;
        }

        /// <summary>
        ///     字符串 转16进制字符串
        /// </summary>
        /// <param name="InString">unico </param>
        /// <returns>类似“01 0f” </returns>
        public static string Str_To_0X(string InString)
        {
            return ByteToString(Encoding.Default.GetBytes(InString));
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
                if (SerialPort != null)
                {
                    if (Linkers.Values.Count(p => p == _com) <= 1)
                    {
                        try
                        {
                            SerialPort.Close();
                        }
                        catch (Exception)
                        {
                            //ignore
                        }
                        SerialPort.Dispose();
                        Connectors[_com] = null;
                        Connectors.Remove(_com);
                    }
                    Linkers.Remove(_slave);
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

        #region 发送接收数据

        public override bool IsConnected
        {
            get
            {
                if (SerialPort != null && !SerialPort.IsOpen)
                {
                    SerialPort.Dispose();
                }
                return SerialPort != null && SerialPort.IsOpen && Linkers.ContainsKey(_slave);
            }
        }

        public override bool Connect()
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
                SerialPort.Open();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public override Task<bool> ConnectAsync()
        {
            return Task.FromResult(Connect());
        }

        public override bool Disconnect()
        {
            if (Linkers.ContainsKey(_slave) && Connectors.ContainsKey(_com))
            {
                try
                {
                    Dispose();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public void SendMsg(string senStr)
        {
            var myByte = StringToByte_2(senStr);

            SendMsg(myByte);
        }

        public override Task<bool> SendMsgWithoutReturnAsync(byte[] message)
        {
            return Task.FromResult(SendMsgWithoutReturn(message));
        }

        public override byte[] SendMsg(byte[] sendbytes)
        {
            try
            {
                if (!SerialPort.IsOpen)
                {
                    try
                    {
                        SerialPort.Open();
                    }
                    catch (Exception)
                    {
                        Dispose();
                        SerialPort.Open();
                    }
                }
                lock (SerialPort.Lock)
                {
                    SerialPort.Write(sendbytes, 0, sendbytes.Length);
                }
                return ReadMsg();
            }
            catch
            {
                Dispose();
                return null;
            }
        }

        public override Task<byte[]> SendMsgAsync(byte[] message)
        {
            return Task.FromResult(SendMsg(message));
        }

        public override bool SendMsgWithoutReturn(byte[] sendbytes)
        {
            try
            {
                if (!SerialPort.IsOpen)
                {
                    try
                    {
                        SerialPort.Open();
                    }
                    catch (Exception)
                    {
                        Dispose();
                        SerialPort.Open();
                    }
                }
                lock (SerialPort.Lock)
                {
                    SerialPort.Write(sendbytes, 0, sendbytes.Length);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string ReadMsgStr()
        {
            var rd = "";

            var data = ReadMsg();

            rd = ByteToString(data);
            return rd;
        }

        public byte[] ReadMsg()
        {
            try
            {
                if (!SerialPort.IsOpen)
                {
                    SerialPort.Open();
                }

                byte[] data;
                Thread.Sleep(100);
                var i = ReadComm(out data, 10, 5000, 1000);
                var returndata = new byte[i];
                Array.Copy(data, 0, returndata, 0, i);
                return returndata;
            }
            catch (Exception)
            {
                Dispose();
                return null;
            }
        }

        #endregion
    }
}