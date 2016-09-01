using System;

namespace Modbus.Net.Siemens
{
    public class SiemensTcpProtocalLinkerBytesExtend : ProtocalLinkerBytesExtend
    {
        public override byte[] BytesExtend(byte[] content)
        {
            Array.Copy(new byte[]{0x03,0x00,0x00,0x00,0x02,0xf0,0x80}, 0, content, 0, 7);
            Array.Copy(BigEndianValueHelper.Instance.GetBytes((ushort)content.Length), 0, content, 2, 2);
            return content;
        }

        public override byte[] BytesDecact(byte[] content)
        {
            byte[] newContent = new byte[content.Length - 7];
            Array.Copy(content, 7, newContent, 0, newContent.Length);
            return newContent;
        }
    }

    public class SiemensPpiProtocalLinkerBytesExtend : ProtocalLinkerBytesExtend
    {
        public override byte[] BytesExtend(byte[] content)
        {
            byte[] newContent = new byte[content.Length + 2];
            Array.Copy(content, 0, newContent, 0, content.Length);
            Array.Copy(new byte[] { 0x68, (byte)(content.Length - 4), (byte)(content.Length - 4), 0x68, 0x02, 0x00 }, 0, newContent, 0, 6);
            int check = 0;
            for (int i = 4; i < newContent.Length - 2; i++)
            {
                check += newContent[i];
            }
            check = check%256;
            newContent[newContent.Length - 2] = (byte) check;
            newContent[newContent.Length - 1] = 0x16;
            return newContent;
        }

        public override byte[] BytesDecact(byte[] content)
        {
            byte[] newContent = new byte[content.Length - 9];
            Array.Copy(content, 7, newContent, 0, newContent.Length);
            return newContent;
        }
    }
}
