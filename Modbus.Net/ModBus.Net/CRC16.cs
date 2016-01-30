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
    }
}
