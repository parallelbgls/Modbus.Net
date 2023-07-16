using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modbus.Net.HJ212
{
    public class HJ212Machine<TKey, TUnitKey> : BaseMachine<TKey, TUnitKey, string, string> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        private static readonly ILogger<HJ212Machine<TKey, TUnitKey>> logger = LogProvider.CreateLogger<HJ212Machine<TKey, TUnitKey>>();

        private readonly int _maxErrorCount = 3;

        protected string ST { get; }

        protected string CN { get; }

        protected string PW { get; }

        protected string MN { get; }

        private int ErrorCount { get; set; }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的地址</param>
        public HJ212Machine(TKey id, string connectionString, string st, string cn, string pw, string mn)
            : base(id, null, true)
        {
            BaseUtility = new HJ212Utility(connectionString);
            ST = st;
            CN = cn;
            PW = pw;
            MN = mn;
        }

        public override async Task<ReturnStruct<bool>> SetDatasAsync(MachineDataType setDataType, Dictionary<string, double> values)
        {
            try
            {
                //检测并连接设备
                if (!BaseUtility.IsConnected)
                    await BaseUtility.ConnectAsync();
                //如果设备无法连接，终止
                if (!BaseUtility.IsConnected) return new ReturnStruct<bool>()
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = -1,
                    ErrorMsg = "Connection Error"
                };

                //遍历每个要设置的值
                Dictionary<string, string> formatValues = new Dictionary<string, string>();
                foreach (var value in values)
                {
                    //根据设置类型找到对应的地址描述
                    formatValues.Add(value.Key, value.Value.ToString());
                }
                var sendValues = new List<Dictionary<string, string>>() { formatValues };
                //写入数据
                await
                    BaseUtility.GetUtilityMethods<IUtilityMethodDatas>().SetDatasAsync("0", new object[] { ST, CN, PW, MN, sendValues, DateTime.Now });

                //如果不保持连接，断开连接
                if (!KeepConnect)
                    BaseUtility.Disconnect();
            }
            catch (Exception e)
            {
                ErrorCount++;
                logger.LogError(e, $"BaseMachine -> SetDatas, Id:{Id} Connection:{ConnectionToken} error. ErrorCount {ErrorCount}.");

                if (ErrorCount >= _maxErrorCount)
                    Disconnect();
                return new ReturnStruct<bool>()
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = "Unknown Exception"
                };
            }
            return new ReturnStruct<bool>()
            {
                Datas = true,
                IsSuccess = true,
                ErrorCode = 0,
                ErrorMsg = ""
            };
        }
    }
}
