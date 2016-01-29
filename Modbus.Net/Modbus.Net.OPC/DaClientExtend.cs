using System;
using System.Threading.Tasks;
using Opc.Da;

namespace Hylasoft.Opc.Da
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
            var result = _server.Read(new[] { item })[0];
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
    }
}
