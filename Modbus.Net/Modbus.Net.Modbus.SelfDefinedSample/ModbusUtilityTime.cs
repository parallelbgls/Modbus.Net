using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus.SelfDefinedSample
{
    public class ModbusUtilityTime : ModbusUtility, IUtilityMethodTime
    {
        private static readonly ILogger<ModbusUtility> logger = LogProvider.CreateLogger<ModbusUtility>();

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionType">协议类型</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusUtilityTime(int connectionType, byte slaveAddress, byte masterAddress,
            Endian endian = Endian.BigEndianLsb)
            : base(connectionType, slaveAddress, masterAddress, endian)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionType">协议类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusUtilityTime(ModbusType connectionType, string connectionString, byte slaveAddress, byte masterAddress,
            Endian endian = Endian.BigEndianLsb)
            : base(connectionType, connectionString, slaveAddress, masterAddress, endian)
        {
        }

        /// <summary>
        ///     读时间
        /// </summary>
        /// <returns>设备的时间</returns>
        public async Task<ReturnStruct<DateTime>> GetTimeAsync()
        {
            try
            {
                var inputStruct = new GetSystemTimeModbusInputStruct(SlaveAddress);
                var outputStruct =
                    await Wrapper.SendReceiveAsync<GetSystemTimeModbusOutputStruct>(
                        Wrapper[typeof(GetSystemTimeModbusProtocol)], inputStruct);
                return new ReturnStruct<DateTime>
                {
                    Datas = outputStruct?.Time ?? DateTime.MinValue,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = ""
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> GetTime: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<DateTime>
                {
                    Datas = DateTime.MinValue,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <summary>
        ///     写时间
        /// </summary>
        /// <param name="setTime">需要写入的时间</param>
        /// <returns>写入是否成功</returns>
        public async Task<ReturnStruct<bool>> SetTimeAsync(DateTime setTime)
        {
            try
            {
                var inputStruct = new SetSystemTimeModbusInputStruct(SlaveAddress, setTime);
                var outputStruct =
                    await Wrapper.SendReceiveAsync<SetSystemTimeModbusOutputStruct>(
                        Wrapper[typeof(SetSystemTimeModbusProtocol)], inputStruct);
                return new ReturnStruct<bool>()
                {
                    Datas = outputStruct?.WriteCount > 0,
                    IsSuccess = outputStruct?.WriteCount > 0,
                    ErrorCode = outputStruct?.WriteCount > 0 ? 0 : -2,
                    ErrorMsg = outputStruct?.WriteCount > 0 ? "" : "Data length zero"
                };
            }
            catch (ModbusProtocolErrorException e)
            {
                logger.LogError(e, $"ModbusUtility -> SetTime: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<bool>
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = e.ErrorMessageNumber,
                    ErrorMsg = e.Message
                };
            }
        }
    }
}
