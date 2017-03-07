using System;
using System.Threading.Tasks;
using Hylasoft.Opc.Common;
using Hylasoft.Opc.Da;
using Opc;
using Opc.Da;
using System.Collections.Generic;
using Hylasoft.Opc.Ua;

namespace Modbus.Net.OPC
{
    public interface IClientExtend : IDisposable
    {
        void Connect();

        T Read<T>(string tag);

        void Write<T>(string tag, T item);

        Task<T> ReadAsync<T>(string tag);

        Task WriteAsync<T>(string tag, T item);

        Task<Node> FindNodeAsync(string tag);

        Task<IEnumerable<Node>> ExploreFolderAsync(string tag);

        Node RootNodeBase { get; }
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
}