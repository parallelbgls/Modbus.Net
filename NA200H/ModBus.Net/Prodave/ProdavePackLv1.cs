using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.Prodave
{
    public class Prodave6
    {
        #region 常值定义（用于极限值）

        public const int MAX_CONNECTIONS = 64; // 64 is default in PRODAVE
        public const int MAX_DEVNAME_LEN = 128; // e.g. "S7ONLINE"
        public const int MAX_BUFFERS = 64; // 64 for blk_read() and blk_write() 
        public const int MAX_BUFFER = 65536; // Transfer buffer for error text) 

        #endregion

        #region 结构体定义

        public struct CON_TABLE_TYPE //待连接plc地址属性表
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] //public CON_ADR_TYPE Adr; // connection address
            public byte[] Adr; // connection address

            // MPI/PB station address (2)
            // IP address (192.168.0.1)
            // MAC address (08-00-06-01-AA-BB)
            public byte AdrType; // Type of address: MPI/PB (1), IP (2), MAC (3)
            public byte SlotNr; // Slot number
            public byte RackNr; // Rack number
        }

        public enum DatType : byte //PLC数据类型
        {

            BYTE = 0x02,
            WORD = 0x04,
            DWORD = 0x06,
        }

        public enum FieldType : byte //PLC区域类型
        {
            //Value types as ASCII characters区域类型对应的ASCII字符
            //data byte (d/D)
            d = 100,
            D = 68,
            //input byte (e/E)
            e = 101,
            E = 69,
            //output byte (a/A)
            a = 97,
            A = 65,
            //memory byte (m/M)
            m = 109,
            M = 77,
            //timer word (t/T),
            t = 116,
            T = 84,
            //counter word (z/Z),
            z = 122,
            Z = 90,
            //virable storage type (v/V),
            v = 118,
            V = 86,
        }

        #endregion

        #region PLC基本函数

        [DllImport("Prodave6.dll")] //连接PLC操作
        //参数：连接号（0-63）、常值"S7ONLINE"、待连接plc地址属性表长度（字节为单位，常值9）、待连接plc地址属性表
        public static extern int LoadConnection_ex6(int ConNr, string pAccessPoint, int ConTableLen,
            ref CON_TABLE_TYPE pConTable);

        [DllImport("Prodave6.dll")] //断开PLC操作
        //参数：连接号（0-63）
        public static extern int UnloadConnection_ex6(UInt16 ConNr);

        [DllImport("Prodave6.dll")] //激活PLC连接操作
        //参数：连接号（0-63）
        public static extern int SetActiveConnection_ex6(UInt16 ConNr);

        [DllImport("Prodave6.dll")] //PLC db区读取操作
        //参数：data block号、要读取的数据类型、起始地址号、需要读取类型的数量、缓冲区长度（字节为单位）、缓冲区、缓冲区数据交互的长度
        public static extern int db_read_ex6(UInt16 BlkNr, DatType DType, UInt16 StartNr, ref UInt32 pAmount,
            UInt32 BufLen,
            byte[] pBuffer, ref UInt32 pDatLen);

        [DllImport("Prodave6.dll")] //PLC db区写入操作
        //参数：data block号、要写入的数据类型、起始地址号、需要写入类型的数量、缓冲区长度（字节为单位）、缓冲区
        public static extern int db_write_ex6(UInt16 BlkNr, DatType Type, UInt16 StartNr, ref UInt32 pAmount,
            UInt32 BufLen,
            byte[] pBuffer);

        [DllImport("Prodave6.dll")] //PLC 任意区读取操作
        //参数：要读取的区类型、data block号(DB区特有，默认为0)、起始地址号、需要读取类型的数量、
        //缓冲区长度（字节为单位）、缓冲区、缓冲区数据交互的长度
        public static extern int field_read_ex6(FieldType FType, UInt16 BlkNr, UInt16 StartNr, UInt32 pAmount,
            UInt32 BufLen,
            byte[] pBuffer, ref UInt32 pDatLen);

        [DllImport("Prodave6.dll")] //PLC 任意区写入操作
        //参数：要写入的区类型、data block号(DB区特有，默认为0)、起始地址号、需要写入类型的数量、
        //缓冲区长度（字节为单位）、缓冲区
        public static extern int field_write_ex6(FieldType FType, UInt16 BlkNr, UInt16 StartNr, UInt32 pAmount,
            UInt32 BufLen,
            byte[] pBuffer);

        [DllImport("Prodave6.dll")] //PLC M区某字节的某位读取操作
        //参数：M区字节号、位号、当前的值(0/1)
        public static extern int mb_bittest_ex6(UInt16 MbNr, UInt16 BitNr, ref int pValue);

        [DllImport("Prodave6.dll")] //PLC M区某字节的某位写入操作
        //参数：M区字节号、位号、要写入的值(0/1)
        public static extern int mb_setbit_ex6(UInt16 MbNr, UInt16 BitNr, byte Value);

        #endregion

        #region PLC200用数据传输函数

        [DllImport("Prodave6.dll")] //200系列PLC 任意区读取操作
        //参数：要读取的区类型、data block号(DB区特有，默认为0)、起始地址号、需要读取类型的数量、
        //缓冲区长度（字节为单位）、缓冲区、缓冲区数据交互的长度
        public static extern int as200_field_read_ex6(FieldType FType, UInt16 BlkNr, UInt16 StartNr, UInt32 pAmount,
            UInt32 BufLen,
            byte[] pBuffer, ref UInt32 pDatLen);

        [DllImport("Prodave6.dll")] //200系列PLC 任意区写入操作
        //参数：要写入的区类型、data block号(DB区特有，默认为0)、起始地址号、需要写入类型的数量、
        //缓冲区长度（字节为单位）、缓冲区
        public static extern int as200_field_write_ex6(FieldType FType, UInt16 BlkNr, UInt16 StartNr, UInt32 pAmount,
            UInt32 BufLen,
            byte[] pBuffer);

        [DllImport("Prodave6.dll")] //200系列PLC M区某字节的某位读取操作
        //参数：M区字节号、位号、当前的值(0/1)
        public static extern int as200_mb_bittest_ex6(UInt16 MbNr, UInt16 BitNr, ref int pValue);

        [DllImport("Prodave6.dll")] //200系列PLC M区某字节的某位写入操作
        //参数：M区字节号、位号、要写入的值(0/1)
        public static extern int as200_mb_setbit_ex6(UInt16 MbNr, UInt16 BitNr, byte Value);

        #endregion

        #region PLC数据转换函数

        [DllImport("Prodave6.dll")] //诊断错误信息操作
        //参数：错误代号、缓冲区大小（字节为单位）、缓冲区
        public static extern int GetErrorMessage_ex6(int ErrorNr, UInt32 BufLen,
            byte[] pBuffer);

        [DllImport("Prodave6.dll")] //S7浮点数转换成PC浮点数
        //参数：S7浮点数、PC浮点数
        public static extern int gp_2_float_ex6(UInt32 gp, ref float pieee);

        [DllImport("Prodave6.dll")] //PC浮点数转换成S7浮点数
        //参数：PC浮点数、S7浮点数
        public static extern int float_2_gp_ex6(float ieee, ref UInt32 pgp);

        [DllImport("Prodave6.dll")] //检测某字节的某位的值是0或1
        //参数：字节值、位号
        public static extern int testbit_ex6(byte Value, int BitNr);

        [DllImport("Prodave6.dll")] //检测某字节的byte值转换成int数组
        //参数：byte值、int数组(长度为8)
        public static extern void byte_2_bool_ex6(byte Value, int[] pBuffer);


        [DllImport("Prodave6.dll")] //检测某字节的int数组转换成byte值
        //参数：int数组(长度为8)
        public static extern byte bool_2_byte_ex6(int[] pBuffer);

        [DllImport("Prodave6.dll")] //交换数据的高低字节——16位数据
        //参数：待交换的数据
        public static extern UInt16 kf_2_integer_ex6(UInt16 wValue); //16位数据——WORD

        [DllImport("Prodave6.dll")] //交换数据的高低字节——32位数据
        //参数：待交换的数据
        public static extern UInt32 kf_2_long_ex6(UInt32 dwValue); //32位数据——DWORD

        [DllImport("Prodave6.dll")] //交换数据缓冲区的的高低字节区，例如pBuffer[0]与pBuffer[1]，pBuffer[2]与pBuffer[3]交换
        //参数：待交换的数据缓冲区，要交换的字节数，如Amount=pBuffer.Length，则交换全部缓冲
        public static extern void swab_buffer_ex6(byte[] pBuffer, UInt32 Amount);

        [DllImport("Prodave6.dll")] //复制数据缓冲区
        //参数：目的数据缓冲区，源数据缓冲区，要复制的数量（字节为单位）
        public static extern void copy_buffer_ex6(byte[] pTargetBuffer, byte[] pSourceBuffer, UInt32 Amount);

        [DllImport("Prodave6.dll")] //把二进制数组传换成BCD码的数组——16位数据
        //参数：要处理的数组，要处理的字节数，转换前是否先交换高低字节，转换后是否要交换高低字节
        //InBytechange为1则转换BCD码之前，先交换高低字节
        //OutBytechange为1则转换BCD码之后，再交换高低字节
        //如果InBytechange和OutBytechange都没有置1，则不发生高低位的交换
        //16位数据BCD码值的许可范围是：+999 —— -999
        public static extern void ushort_2_bcd_ex6(UInt16[] pwValues, UInt32 Amount, int InBytechange, int OutBytechange); //16位数据——WORD

        [DllImport("Prodave6.dll")] //把二进制数组传换成BCD码的数组——32位数据
        //参数：要处理的数组，要处理的字节数，转换前是否先交换高低字节，转换后是否要交换高低字节
        //InBytechange为1则转换BCD码之前，先交换高低字节
        //OutBytechange为1则转换BCD码之后，再交换高低字节
        //如果InBytechange和OutBytechange都没有置1，则不发生高低位的交换
        //32位数据BCD码值的许可范围是：+9 999 999 —— -9 999 999
        public static extern void ulong_2_bcd_ex6(UInt32[] pdwValues, UInt32 Amount, int InBytechange, int OutBytechange); //32位数据——DWORD

        [DllImport("Prodave6.dll")] //把BCD码的数组传换成二进制数组——16位数据
        //参数：要处理的数组，要处理的字节数，转换前是否先交换高低字节，转换后是否要交换高低字节
        //InBytechange为1则转换BCD码之前，先交换高低字节
        //OutBytechange为1则转换BCD码之后，再交换高低字节
        //如果InBytechange和OutBytechange都没有置1，则不发生高低位的交换
        //16位数据BCD码值的许可范围是：+999 —— -999
        public static extern void bcd_2_ushort_ex6(UInt16[] pwValues, UInt32 Amount, int InBytechange, int OutBytechange); //16位数据——WORD

        [DllImport("Prodave6.dll")] //把BCD码的数组传换成二进制数组——32位数据
        //参数：要处理的数组，要处理的字节数，转换前是否先交换高低字节，转换后是否要交换高低字节
        //InBytechange为1则转换BCD码之前，先交换高低字节
        //OutBytechange为1则转换BCD码之后，再交换高低字节
        //如果InBytechange和OutBytechange都没有置1，则不发生高低位的交换
        //32位数据BCD码值的许可范围是：+9 999 999 —— -9 999 999
        public static extern void bcd_2_ulong_ex6(UInt32[] pdwValues, UInt32 Amount, int InBytechange, int OutBytechange); //32位数据——DWORD

        [DllImport("Prodave6.dll")] //查看64个连接中哪些被占用，哪些已经建立
        //参数：传输缓冲的字节长度，64位长度的数组(0或1)
        public static extern void GetLoadedConnections_ex6(UInt32 BufLen, int[] pBuffer);

        #endregion

        #region 自定义辅助函数

        public static UInt16 bytes_2_word(byte dbb0, byte dbb1) //将高低2个byte转换成1个word
        {
            UInt16 dbw0;
            dbw0 = (UInt16) (dbb0*256 + dbb1);
            return dbw0;
        }

        public static UInt32 bytes_2_dword(byte dbb0, byte dbb1, byte dbb2, byte dbb3) //将高低4个byte转换成1个dword
        {
            UInt32 dbd0;
            dbd0 = (UInt32) (dbb0*16777216 + dbb1*65536 + dbb2*256 + dbb3);
            return dbd0;
        }

        public static UInt32 words_2_dword(UInt16 dbw0, UInt16 dbw2) //将高低2个word转换成1个dword
        {
            UInt32 dbd0;
            dbd0 = (UInt32) (dbw0*65536 + dbw2);
            return dbd0;
        }

        public static byte[] word_2_bytes(UInt16 dbw0) //将word拆分为2个byte
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte) (dbw0/256);
            bytes[1] = (byte) (dbw0%256);
            return bytes;
        }

        public static byte[] dword_2_bytes(UInt32 dbd0) //将dword拆分为4个byte
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte) (dbd0/16777216);
            dbd0 = dbd0%16777216;
            bytes[1] = (byte) (dbd0/65536);
            dbd0 = dbd0%65536;
            bytes[2] = (byte) (dbd0/256);
            bytes[3] = (byte) (dbd0%256);
            return bytes;
        }

        public static UInt16[] dword_2_words(UInt32 dbd0) //将dword拆分为2个word
        {
            UInt16[] words = new UInt16[2];
            words[0] = (UInt16) (dbd0/65536);
            words[1] = (UInt16) (dbd0%65536);
            return words;
        }

        #endregion

    }

}
