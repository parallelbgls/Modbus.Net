using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public class ComConnector : BaseConnector, IDisposable
    {
        public override string ConnectionToken => _com;

        private SerialPort _serialPort1;

        private SerialPort SerialPort1
        {
            get
            {
                if (_serialPort1 == null)
                {
                    _serialPort1 = new SerialPort
                    {
                        PortName = _com,
                        BaudRate = _baudRate,
                        Parity = _parity,
                        StopBits = _stopBits,
                        DataBits = _dataBits,
                        ReadTimeout = _timeoutTime,
                    };
                }
                return _serialPort1;
            }
            
        }

        public delegate byte[] GetDate(byte[] bts);

        //private GetDate mygetDate;
        private readonly string _com;
        private readonly int _timeoutTime;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly StopBits _stopBits;
        private readonly int _dataBits;

        public ComConnector(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits, int timeoutTime)
        {
            _com = com;
            _timeoutTime = timeoutTime;
            _baudRate = baudRate;
            _parity = parity;
            _stopBits = stopBits;
            _dataBits = dataBits;

            //端口号 
            //比特率 
            //奇偶校验 
            //停止位 
            //读超时，即在1000内未读到数据就引起超时异常 
        }

        #region 发送接收数据

        public override bool IsConnected
        {
            get
            {
                if (_serialPort1 != null && !SerialPort1.IsOpen)
                {
                    _serialPort1.Dispose();
                    _serialPort1 = null;
                }
                return _serialPort1 != null && _serialPort1.IsOpen;
            }
        }

        public override bool Connect()
        {
            try
            {
                SerialPort1.Open();
                return true;
            }
            catch
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
            if (SerialPort1 != null)
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
            byte[] myByte = StringToByte_2(senStr);

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
                if (!SerialPort1.IsOpen)
                {
                    try
                    {
                        SerialPort1.Open();
                    }
                    catch (Exception)
                    {
                        Dispose();
                        SerialPort1.Open();
                    }                    
                }
                SerialPort1.Write(sendbytes, 0, sendbytes.Length);
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
                SerialPort1.Write(sendbytes, 0, sendbytes.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
 
        }

        public string ReadMsgStr()
        {
            string rd = "";

            byte[] data = ReadMsg();

            rd = ByteToString(data);
            return rd;

        }

        public byte[] ReadMsg()
        {
            try
            {
                if (!SerialPort1.IsOpen)
                {
                    SerialPort1.Open();
                }

                byte[] data;
                Thread.Sleep(100);
                int i = ReadComm(out data, 10, 5000, 1000);
                byte[] returndata = new byte[i];
                Array.Copy(data, 0, returndata, 0, i);
                return returndata;
            }
            catch (Exception ex)
            {
                Dispose();
                return null;
            }     
        }

        #endregion

        ///  <summary> 
        /// 串口读(非阻塞方式读串口，直到串口缓冲区中没有数据 
        ///  </summary> 
        ///  <param name="readBuf">串口数据缓冲 </param> 
        ///  <param name="bufRoom">串口数据缓冲空间大小 </param> 
        ///  <param name="HowTime">设置串口读放弃时间 </param> 
        ///  <param name="ByteTime">字节间隔最大时间 </param> 
        ///  <returns>串口实际读入数据个数 </returns> 
        public int ReadComm(out Byte[] readBuf, int bufRoom, int HowTime, int ByteTime)
        {
            //throw new System.NotImplementedException(); 
            readBuf = new Byte[1023];
            Array.Clear(readBuf, 0, readBuf.Length);

            int nReadLen, nBytelen;
            if (SerialPort1.IsOpen == false)
                return -1;
            nBytelen = 0;
            SerialPort1.ReadTimeout = HowTime;


            try
            {
                while (SerialPort1.BytesToRead > 0)
                {
                    readBuf[nBytelen] = (byte) SerialPort1.ReadByte();
                    byte[] bTmp = new byte[bufRoom];
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

        ///  <summary> 
        /// 串口同步读(阻塞方式读串口，直到串口缓冲区中没有数据,靠字符间间隔超时确定没有数据) 
        ///  </summary> 
        ///  <param name="ReadBuf">串口数据缓冲 </param> 
        ///  <param name="ReadRoom">串口数据缓冲空间大小 </param> 
        ///  <param name="ByteTime">字节间隔最大时间 </param> 
        ///  <returns>从串口实际读入的字节个数 </returns> 
        public int ReadBlock(byte[] ReadBuf, int ReadRoom, int ByteTime)
        {
            sbyte nBytelen;
            //long nByteRead; 

            if (SerialPort1.IsOpen == false)
                return 0;
            nBytelen = 0;
            SerialPort1.ReadTimeout = ByteTime;

            while (nBytelen < ReadRoom - 1 && SerialPort1.BytesToRead > 0)
            {
                try
                {
                    ReadBuf[nBytelen] = (byte) SerialPort1.ReadByte();
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


        ///  <summary> 
        /// 字符数组转字符串16进制 
        ///  </summary> 
        ///  <param name="InBytes"> 二进制字节 </param> 
        ///  <returns>类似"01 02 0F" </returns> 
        public static string ByteToString(byte[] InBytes)
        {
            string StringOut = "";
            foreach (byte InByte in InBytes)
            {
                StringOut = StringOut + String.Format("{0:X2}", InByte) + " ";
            }

            return StringOut.Trim();
        }

        ///  <summary> 
        /// strhex 转字节数组 
        ///  </summary> 
        ///  <param name="InString">类似"01 02 0F" 用空格分开的  </param> 
        ///  <returns> </returns> 
        public static byte[] StringToByte(string InString)
        {
            string[] ByteStrings;
            ByteStrings = InString.Split(" ".ToCharArray());
            byte[] ByteOut;
            ByteOut = new byte[ByteStrings.Length];
            for (int i = 0; i <= ByteStrings.Length - 1; i++)
            {
                ByteOut[i] = byte.Parse(ByteStrings[i], System.Globalization.NumberStyles.HexNumber);
            }
            return ByteOut;
        }

        ///  <summary> 
        /// strhex 转字节数组 
        ///  </summary> 
        ///  <param name="InString">类似"01 02 0F" 中间无空格 </param> 
        ///  <returns> </returns> 
        public static byte[] StringToByte_2(string InString)
        {
            byte[] ByteOut;
            InString = InString.Replace(" ", "");
            try
            {
                string[] ByteStrings = new string[InString.Length/2];
                int j = 0;
                for (int i = 0; i < ByteStrings.Length; i++)
                {

                    ByteStrings[i] = InString.Substring(j, 2);
                    j += 2;
                }

                ByteOut = new byte[ByteStrings.Length];
                for (int i = 0; i <= ByteStrings.Length - 1; i++)
                {
                    ByteOut[i] = byte.Parse(ByteStrings[i], System.Globalization.NumberStyles.HexNumber);
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

            return ByteOut;
        }

        ///  <summary> 
        /// 字符串 转16进制字符串 
        ///  </summary> 
        ///  <param name="InString">unico </param> 
        ///  <returns>类似“01 0f” </returns> 
        public static string Str_To_0X(string InString)
        {
            return ByteToString(UnicodeEncoding.Default.GetBytes(InString));
        }

        public void Dispose()
        {
            if (_serialPort1 != null)
            {
                _serialPort1.Close();
                _serialPort1.Dispose();
                _serialPort1 = null;
            }
        }
    }
}
