using System.Text;

namespace Modbus.Net
{
    /// <summary>
    ///     数据检查类
    /// </summary>
    public static class ContentCheck
    {
        /// <summary>
        ///     检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public static bool? CheckRight(byte[] content)
        {
            if (content == null || content.Length == 0)
            {
                return null;
            }
            return true;
        }

        /// <summary>
        ///     Lrc校验
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public static bool? LrcCheckRight(byte[] content)
        {
            var baseCheck = CheckRight(content);
            if (baseCheck != true) return baseCheck;
            var contentString = Encoding.ASCII.GetString(content);
            if (!Crc16.GetInstance().LrcEfficacy(contentString))
                return false;
            return true;
        }

        /// <summary>
        ///     Crc16校验
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public static bool? Crc16CheckRight(byte[] content)
        {
            var baseCheck = CheckRight(content);
            if (baseCheck != true) return baseCheck;
            if (!Crc16.GetInstance().CrcEfficacy(content))
                return false;
            return true;
        }

        /// <summary>
        ///     Fcs校验
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public static bool? FcsCheckRight(byte[] content)
        {
            var fcsCheck = 0;
            var start = content[0] == 0x10 ? 1 : 4;
            if (content[0] == 0xE5) return true;
            for (var i = start; i < content.Length - 2; i++)
                fcsCheck += content[i];
            fcsCheck = fcsCheck % 256;
            if (fcsCheck != content[content.Length - 2]) return false;
            return true;
        }
    }
}
