using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modbus.Net.HJ212
{
    public class HJ212Utility : BaseUtility<byte[], byte[], ProtocolUnit<byte[], byte[]>, PipeUnit>
    {
        private static readonly ILogger<HJ212Utility> logger = LogProvider.CreateLogger<HJ212Utility>();

        public HJ212Utility(string connectionString) : base(0, 0)
        {
            ConnectionString = connectionString;
            Wrapper = new HJ212Protocol(connectionString);
        }

        public override Endian Endian => throw new NotImplementedException();

        public override Task<ReturnStruct<byte[]>> GetDatasAsync(string startAddress, int getByteCount)
        {
            throw new NotImplementedException();
        }

        public override void SetConnectionType(int connectionType)
        {
            throw new NotImplementedException();
        }

        public override async Task<ReturnStruct<bool>> SetDatasAsync(string startAddress, object[] setContents)
        {
            try
            {
                var writeRequestHJ212InputStruct =
                    new WriteRequestHJ212InputStruct((string)setContents[0], (string)setContents[1], (string)setContents[2], (string)setContents[3], (List<Dictionary<string, string>>)setContents[4], (DateTime)setContents[5]);
                var writeRequestOpcOutputStruct =
                    await
                        Wrapper.SendReceiveAsync<WriteRequestHJ212OutputStruct>(Wrapper[typeof(WriteRequestHJ212Protocol)],
                            writeRequestHJ212InputStruct);
                return new ReturnStruct<bool>
                {
                    Datas = writeRequestOpcOutputStruct?.GetValue != null,
                    IsSuccess = writeRequestOpcOutputStruct?.GetValue != null,
                    ErrorCode = 0,
                    ErrorMsg = null,
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"OpcUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<bool>
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = e.Message
                };
            }
        }
    }
}
