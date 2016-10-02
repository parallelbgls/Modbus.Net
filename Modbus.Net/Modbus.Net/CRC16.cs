using System;

namespace Modbus.Net
{
    public class Crc16
    {
        /// <summary>
        /// CRC验证表
        /// </summary>
        public byte[] crc_table = new byte[512];

        private static Crc16 _crc16 = null;

        public static Crc16 GetInstance()
        {
            if (_crc16 == null)
            {
                _crc16 = new Crc16();
            }
            return _crc16;
        }

        #region 生成CRC码

        /// <summary>
        /// 生成CRC码
        /// </summary>
        /// <param name="message">发送或返回的命令，CRC码除外</param>
        /// <param name="Rcvbuf">存储CRC码的字节的数组</param>
        /// <param name="CRC">生成的CRC码</param>
        public short GetCRC(byte[] message, ref byte[] Rcvbuf)
        {
            int  IX,IY,CRC;
            int Len = message.Length;
            CRC=0xFFFF;
            //set all 1     
            if (Len<=0)         
                CRC = 0;      
            else      
            {         
                Len--;                     
                for (IX=0;IX<=Len;IX++)         
                {                   
                    CRC=CRC^(message[IX]);       
                    for(IY=0;IY<=7;IY++)           
                    {                           
                        if ((CRC&1)!=0 )         
                            CRC=(CRC>>1)^0xA001;           
                        else                        
                            CRC=CRC>>1;    
                        //            
                    }             
                }        
            }        
            Rcvbuf[1] = (byte)((CRC & 0xff00)>>8);//高位置  
            Rcvbuf[0] = (byte)(CRC & 0x00ff);  //低位置 
            CRC= Rcvbuf[0]<<8;    
            CRC+= Rcvbuf[1];     
            return (short)CRC;
        }

        #endregion

        #region CRC验证

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="src">ST开头，&&结尾</param>
        /// <returns>十六进制数</returns>
        public bool CrcEfficacy(byte[] byteframe)
        {
            byte[] recvbuff = new byte[2];
            byte[] byteArr = new byte[byteframe.Length - 2];
            Array.Copy(byteframe, 0, byteArr, 0, byteArr.Length);
            GetCRC(byteArr, ref recvbuff);
            if (recvbuff[0] == byteframe[byteframe.Length - 2] && recvbuff[1] == byteframe[byteframe.Length - 1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// 取模FF(255)
        /// 取反+1
        /// </summary>
        /// <param name="writeUncheck"></param>
        /// <returns></returns>
        public bool LrcEfficacy(string message)
        {
            var index = message.IndexOf(Environment.NewLine, StringComparison.InvariantCulture);
            var writeUncheck = message.Substring(1, index - 2);
            var checkString = message.Substring(index - 2, 2);
            char[] hexArray = new char[writeUncheck.Length];
            hexArray = writeUncheck.ToCharArray();
            int decNum = 0, decNumMSB = 0, decNumLSB = 0;
            int decByte, decByteTotal = 0;

            bool msb = true;

            for (int t = 0; t <= hexArray.GetUpperBound(0); t++)
            {
                if ((hexArray[t] >= 48) && (hexArray[t] <= 57))

                    decNum = (hexArray[t] - 48);

                else if ((hexArray[t] >= 65) & (hexArray[t] <= 70))
                    decNum = 10 + (hexArray[t] - 65);

                if (msb)
                {
                    decNumMSB = decNum * 16;
                    msb = false;
                }
                else
                {
                    decNumLSB = decNum;
                    msb = true;
                }
                if (msb)
                {
                    decByte = decNumMSB + decNumLSB;
                    decByteTotal += decByte;
                }
            }

            decByteTotal = (255 - decByteTotal) + 1;
            decByteTotal = decByteTotal & 255;

            int a, b = 0;

            string hexByte = "", hexTotal = "";
            double i;

            for (i = 0; decByteTotal > 0; i++)
            {
                //b = Convert.ToInt32(System.Math.Pow(16.0, i));
                a = decByteTotal % 16;
                decByteTotal /= 16;
                if (a <= 9)
                    hexByte = a.ToString();
                else
                {
                    switch (a)
                    {
                        case 10:
                            hexByte = "A";
                            break;
                        case 11:
                            hexByte = "B";
                            break;
                        case 12:
                            hexByte = "C";
                            break;
                        case 13:
                            hexByte = "D";
                            break;
                        case 14:
                            hexByte = "E";
                            break;
                        case 15:
                            hexByte = "F";
                            break;
                    }
                }
                hexTotal = String.Concat(hexByte, hexTotal);
            }
            return hexTotal == checkString;
        }

        public string GetLRC(byte[] code)
        {
            byte sum = 0;
            foreach (byte b in code)
            {
                sum += b;
            }
            sum = (byte)(~sum + 1);//取反+1
            string lrc = sum.ToString("X2");
            return lrc;
        }
    }
}
