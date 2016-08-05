using System;
using System.Threading.Tasks;
using Hylasoft.Opc.Common;
using Hylasoft.Opc.Da;
using Opc;
using Opc.Da;

namespace Modbus.Net.OPC
{

    /// <summary>
    /// Read value full result
    /// </summary>
    public class OpcValueResult
    {
        public object Value { get; set; }
        public DateTime Timestamp { get; set; }
        public bool QualityGood { get; set; }
    }

    public class MyDaClient : DaClient
    {
        /// <summary>
        /// Write a value on the specified opc tag
        /// </summary>
        /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
        /// E.g: the tag `foo.bar` writes on the tag `bar` on the folder `foo`</param>
        public OpcValueResult Read(string tag)
        {
            var item = new Item { ItemName = tag };
            var result = Server.Read(new[] { item })[0];
            CheckResult(result, tag);
            return new OpcValueResult()
            {
                Value = result.Value,
                Timestamp = result.Timestamp,
                QualityGood = result.Quality == Quality.Good
            };
        }

        /// <summary>
        /// Read a tag asynchronusly
        /// </summary>
        public Task<OpcValueResult> ReadAsync(string tag)
        {
            return Task.Run(() => Read(tag));
        }

        public MyDaClient(Uri serverUrl) : base(serverUrl)
        {
        }

        private static void CheckResult(IResult result, string tag)
        {
            if (result == null)
                throw new OpcException("The server replied with an empty response");
            if (result.ResultID.ToString() != "S_OK")
                throw new OpcException(string.Format("Invalid response from the server. (Response Status: {0}, Opc Tag: {1})", result.ResultID, tag));
        }
    }
}
