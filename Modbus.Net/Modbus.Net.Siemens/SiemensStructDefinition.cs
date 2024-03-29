﻿/*namespace Modbus.Net.Siemens
{
    public struct TodClockStatus
    {
        public bool KV 
        {
            get
            {
                var pos = 15;
                return BigEndianLsbValueHelper.Instance.GetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), ref pos);
            }
            set { TodValue = BigEndianLsbValueHelper.Instance.SetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), 15, value); } 
        }

        public byte K0_4
        {
            get
            {
                var pos = 0;
                var byteValue = BigEndianLsbValueHelper.Instance.GetByte(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), ref pos);
                return (byte)(byteValue%64/4);
            }
            set
            {
                var pos = 0;
                var byteValue = BigEndianLsbValueHelper.Instance.GetByte(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), ref pos);
                byteValue = (byte)(byteValue - (byteValue%128/4) + value);
                TodValue = (ushort)(TodValue%128 + byteValue*128);
            }
        }

        public bool ZNA
        {
            get
            {
                var pos = 5;
                return BigEndianLsbValueHelper.Instance.GetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), ref pos);
            }
            set { TodValue = BigEndianLsbValueHelper.Instance.SetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), 5, value); }
        }

        public byte UA
        {
            get
            {
                var pos = 3;
                var low = BigEndianLsbValueHelper.Instance.GetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), ref pos) ? 1 : 0;
                var high = BigEndianLsbValueHelper.Instance.GetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), ref pos) ? 1 : 0;
                high *= 2;
                return (byte) (high + low);
            }
            set
            {
                TodValue = BigEndianLsbValueHelper.Instance.SetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), 3, value % 2 >= 1);
                TodValue = BigEndianLsbValueHelper.Instance.SetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), 4, value / 2 >= 1);
            }
        }
        public bool UZS
        {
            get
            {
                var pos = 2;
                return BigEndianLsbValueHelper.Instance.GetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), ref pos);
            }
            set { TodValue = BigEndianLsbValueHelper.Instance.SetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), 2, value); }
        }

        public bool ESY
        {
            get
            {
                var pos = 1;
                return BigEndianLsbValueHelper.Instance.GetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), ref pos);
            }
            set { TodValue = BigEndianLsbValueHelper.Instance.SetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), 1, value); }
        }

        public bool SYA
        {
            get
            {
                var pos = 0;
                return BigEndianLsbValueHelper.Instance.GetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), ref pos);
            }
            set { TodValue = BigEndianLsbValueHelper.Instance.SetBit(BigEndianLsbValueHelper.Instance.GetBytes(TodValue), 0, value); }
        }

        public ushort TodValue { get; set; }
    }
}
*/

