using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     异常状态获取方法
    /// </summary>
    public interface IUtilityMethodExceptionStatus
    {
        /// <summary>
        ///     获取异常状态
        /// </summary>
        /// <returns></returns>
        Task<ReturnStruct<byte>> GetExceptionStatusAsync();
    }

    /// <summary>
    ///     诊断返回数据
    /// </summary>
    public class DiagnoticsData
    {
        /// <summary>
        ///     子方法编号
        /// </summary>
        public ushort SubFunction { get; set; }

        /// <summary>
        ///     诊断数据
        /// </summary>
        public ushort[] Data { get; set; }
    }

    /// <summary>
    ///     诊断获取方法
    /// </summary>
    public interface IUtilityMethodDiagnotics
    {
        /// <summary>
        ///     获取诊断信息
        /// </summary>
        /// <param name="subFunction">子方法编号</param>
        /// <param name="data">诊断数据</param>
        /// <returns></returns>
        Task<ReturnStruct<DiagnoticsData>> GetDiagnoticsAsync(ushort subFunction, ushort[] data);
    }

    /// <summary>
    ///     通讯事件计数器获取数据
    /// </summary>
    public class CommEventCounterData
    {
        /// <summary>
        ///     通讯状态
        /// </summary>
        public ushort Status { get; set; }

        /// <summary>
        ///     事件计数
        /// </summary>
        public ushort EventCount { get; set; }
    }

    /// <summary>
    ///     通讯事件计数器获取方法
    /// </summary>
    public interface IUtilityMethodCommEventCounter
    {
        /// <summary>
        ///     获取通讯事件计数器
        /// </summary>
        /// <returns></returns>
        Task<ReturnStruct<CommEventCounterData>> GetCommEventCounterAsync();
    }

    /// <summary>
    ///     通讯事件获取数据
    /// </summary>
    public class CommEventLogData
    {
        /// <summary>
        ///     状态
        /// </summary>
        public ushort Status { get; set; }

        /// <summary>
        ///     事件内容
        /// </summary>
        public byte[] Events { get; set; }
    }

    /// <summary>
    ///     通讯事件获取方法
    /// </summary>
    public interface IUtilityMethodCommEventLog
    {
        /// <summary>
        ///     获取通讯事件
        /// </summary>
        /// <returns></returns>
        Task<ReturnStruct<CommEventLogData>> GetCommEventLogAsync();
    }

    /// <summary>
    ///     获取从站号数据
    /// </summary>
    public class SlaveIdData
    {
        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveId { get; set; }

        /// <summary>
        ///     指示状态
        /// </summary>
        public byte IndicatorStatus { get; set; }

        /// <summary>
        ///     附加信息
        /// </summary>
        public byte[] AdditionalData { get; set; }
    }

    /// <summary>
    ///     获取从站号方法
    /// </summary>
    public interface IUtilityMethodSlaveId
    {
        /// <summary>
        ///     获取从站号
        /// </summary>
        /// <returns></returns>
        Task<ReturnStruct<SlaveIdData>> GetSlaveIdAsync();
    }

    /// <summary>
    ///     文件记录读写方法
    /// </summary>
    public interface IUtilityMethodFileRecord
    {
        /// <summary>
        ///     读文件记录
        /// </summary>
        /// <param name="recordDefs">读文件记录定义</param>
        /// <returns></returns>
        Task<ReturnStruct<ReadFileRecordOutputDef[]>> GetFileRecordAsync(ReadFileRecordInputDef[] recordDefs);
        /// <summary>
        ///     写文件记录
        /// </summary>
        /// <param name="recordDefs">写文件记录定义</param>
        /// <returns></returns>
        Task<ReturnStruct<WriteFileRecordOutputDef[]>> SetFileRecordAsync(WriteFileRecordInputDef[] recordDefs);
    }

    /// <summary>
    ///     掩码写入数据
    /// </summary>
    public class MaskRegisterData
    {
        /// <summary>
        ///     地址索引
        /// </summary>
        public ushort ReferenceAddress { get; set; }

        /// <summary>
        ///     与掩码
        /// </summary>
        public ushort AndMask { get; set; }

        /// <summary>
        ///     或掩码
        /// </summary>
        public ushort OrMask { get; set; }
    }

    /// <summary>
    ///     掩码写入方法
    /// </summary>
    public interface IUtilityMethodMaskRegister
    {
        /// <summary>
        ///     写入掩码
        /// </summary>
        /// <param name="referenceAddress">地址索引</param>
        /// <param name="andMask">与掩码</param>
        /// <param name="orMask">或掩码</param>
        /// <returns></returns>
        Task<ReturnStruct<MaskRegisterData>> SetMaskRegister(ushort referenceAddress, ushort andMask, ushort orMask);
    }

    /// <summary>
    ///     寄存器读写方法
    /// </summary>
    public interface IUtilityMethodMultipleRegister
    {
        /// <summary>
        ///     读写多寄存器
        /// </summary>
        /// <param name="readStartingAddress">读起始地址</param>
        /// <param name="quantityToRead">读数量</param>
        /// <param name="writeStartingAddress">写寄存器地址</param>
        /// <param name="writeValues">写数据</param>
        /// <returns></returns>
        Task<ReturnStruct<ushort[]>> GetMultipleRegister(ushort readStartingAddress, ushort quantityToRead, ushort writeStartingAddress, ushort[] writeValues);
    }

    /// <summary>
    ///     FIFO队列读取方法
    /// </summary>
    public interface IUtilityMethodFIFOQueue
    {
        /// <summary>
        ///     读FIFO队列
        /// </summary>
        /// <param name="fifoPointerAddress">FIFO队列地址</param>
        /// <returns></returns>
        Task<ReturnStruct<ushort[]>> GetFIFOQueue(ushort fifoPointerAddress);
    }
}
