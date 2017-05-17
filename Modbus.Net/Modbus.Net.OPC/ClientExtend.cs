using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hylasoft.Opc.Common;
using Hylasoft.Opc.Da;
using Hylasoft.Opc.Ua;

namespace Modbus.Net.OPC
{
    public interface IClientExtend : IDisposable
    {
        Node RootNodeBase { get; }

        void Connect();

        T Read<T>(string tag);

        void Write<T>(string tag, T item);

        Task<T> ReadAsync<T>(string tag);

        Task WriteAsync<T>(string tag, T item);

        Task<Node> FindNodeAsync(string tag);

        Task<IEnumerable<Node>> ExploreFolderAsync(string tag);
    }

    public class MyDaClient : DaClient, IClientExtend
    {
        public MyDaClient(Uri serverUrl) : base(serverUrl)
        {
        }

        public Node RootNodeBase => RootNode;
    }

    public class MyUaClient : UaClient, IClientExtend
    {
        public MyUaClient(Uri serverUrl) : base(serverUrl)
        {
        }

        public Node RootNodeBase => RootNode;
    }

    public class OpcParamIn
    {
        public bool IsRead { get; set; }
        public string Tag { get; set; }
        public char Split { get; set; }
        public object SetValue { get; set; }
    }

    public class OpcParamOut
    {
        public bool Success { get; set; }
        public byte[] Value { get; set; }
    }
}