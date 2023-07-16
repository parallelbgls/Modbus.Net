using System;

namespace Modbus.Net
{
    /// <summary>
    ///     端格式
    /// </summary>
    public partial class Endian
    {
        /// <summary>
        ///     小端
        /// </summary>
        public const int LittleEndianLsb = 1;
        /// <summary>
        ///     大端-大端位
        /// </summary>
        public const int BigEndianLsb = 2;
        /// <summary>
        ///     大端-大端位
        /// </summary>
        public const int BigEndianMsb = 3;

#pragma warning disable
        protected int Value { get; set; }

        protected Endian(int value)
        {
            Value = value;
        }

        public static implicit operator int(Endian endian)
        {
            return endian.Value;
        }

        public static implicit operator Endian(int value)
        {
            return new Endian(value);
        }

        public static implicit operator Endian(string value)
        {
            return Endian.Parse(value);
        }

        public static implicit operator string(Endian endian)
        {
            return endian.ToString();
        }

        public static Endian Parse(string value)
        {
            var Assemblies = AssemblyHelper.GetAllLibraryAssemblies();
            foreach (var assembly in Assemblies)
            {
                if (assembly.GetType("Modbus.Net.Endian")?.GetField(value) != null)
                {
                    return (int)assembly.GetType("Modbus.Net.Endian").GetField(value).GetValue(null);
                }
            }
            throw new NotSupportedException("Endian name " + value + " is not supported.");
        }

        public override string ToString()
        {
            var Assemblies = AssemblyHelper.GetAllLibraryAssemblies();
            foreach (var assembly in Assemblies)
            {
                var endianType = assembly.GetType("Modbus.Net.Endian");
                if (endianType != null)
                {
                    foreach (var field in endianType.GetFields())
                    {
                        if ((int)field.GetValue(null) == Value)
                        {
                            return field.Name;
                        }
                    }
                }
            }
            return null;
        }
#pragma warning restore
    }

    /// <summary>
    ///     读写设备值的方式
    /// </summary>
    public enum MachineDataType
    {
        /// <summary>
        ///     地址
        /// </summary>
        Address,

        /// <summary>
        ///     通讯标识
        /// </summary>
        CommunicationTag,

        /// <summary>
        ///     名称
        /// </summary>
        Name,

        /// <summary>
        ///     Id
        /// </summary>
        Id
    }

    /// <summary>
    ///     波特率
    /// </summary>
    public enum BaudRate
    {
#pragma warning disable
        BaudRate75 = 75,
        BaudRate110 = 110,
        BaudRate134 = 134,
        BaudRate150 = 150,
        BaudRate300 = 300,
        BaudRate600 = 600,
        BaudRate1200 = 1200,
        BaudRate1800 = 1800,
        BaudRate2400 = 2400,
        BaudRate4800 = 4800,
        BaudRate9600 = 9600,
        BaudRate14400 = 14400,
        BaudRate19200 = 19200,
        BaudRate38400 = 38400,
        BaudRate57600 = 57600,
        BaudRate115200 = 115200,
        BaudRate128000 = 128000,
        BaudRate230400 = 230400,
        BaudRate460800 = 460800,
        BaudRate921600 = 921600,
#pragma warning restore
    }

    /// <summary>
    ///     数据位
    /// </summary>
    public enum DataBits
    {
#pragma warning disable
        Seven = 7,
        Eight = 8,
#pragma warning restore
    }

}
