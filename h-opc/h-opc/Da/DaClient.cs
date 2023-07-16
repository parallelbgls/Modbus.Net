using Hylasoft.Opc.Common;
using System.Globalization;
using Technosoftware.DaAeHdaClient;
using Factory = Technosoftware.DaAeHdaClient.Com.Factory;
using OpcDa = Technosoftware.DaAeHdaClient.Da;

namespace Hylasoft.Opc.Da
{
    /// <summary>
    /// Client Implementation for DA
    /// </summary>
    public partial class DaClient : IClient<DaNode>
    {
        private readonly OpcUrl _url;
        private OpcDa.TsCDaServer _server;
        private long _sub;
        private readonly IDictionary<string, DaNode> _nodesCache = new Dictionary<string, DaNode>();

        // default monitor interval in Milliseconds
        private const int DefaultMonitorInterval = 100;

        /// <summary>
        /// Initialize a new Data Access Client
        /// </summary>
        /// <param name="serverUrl">The url of the server to connect to. WARNING: If server URL includes
        /// spaces (ex. "RSLinx OPC Server") then pass the server URL in to the constructor as an Opc.URL object
        /// directly instead.</param>
        public DaClient(Uri serverUrl)
        {
            _url = new OpcUrl(serverUrl.OriginalString)
            {
                Scheme = serverUrl.Scheme,
                HostName = serverUrl.Host
            };
        }

        /// <summary>
        /// Initialize a new Data Access Client
        /// </summary>
        /// <param name="serverUrl">The url of the server to connect to</param>
        public DaClient(OpcUrl serverUrl)
        {
            _url = serverUrl;
        }

        /// <summary>
        /// Gets the datatype of an OPC tag
        /// </summary>
        /// <param name="tag">Tag to get datatype of</param>
        /// <returns>System Type</returns>
        public System.Type GetDataType(string tag)
        {
            var item = new OpcDa.TsCDaItem { ItemName = tag };
            OpcDa.TsCDaItemProperty result;
            try
            {
                var propertyCollection = _server.GetProperties(new[] { item }, new[] { new OpcDa.TsDaPropertyID(1) }, false)[0];
                result = propertyCollection[0];
            }
            catch (NullReferenceException)
            {
                throw new OpcException("Could not find node because server not connected.");
            }
            return result.DataType;
        }

        /// <summary>
        /// OpcDa underlying server object.
        /// </summary>
        protected OpcDa.TsCDaServer Server
        {
            get
            {
                return _server;
            }
        }

        #region interface methods

        /// <summary>
        /// Connect the client to the OPC Server
        /// </summary>
        public void Connect()
        {
            if (Status == OpcStatus.Connected)
                return;
            _server = new OpcDa.TsCDaServer(new Factory(), _url);
            _server.Connect();
            var root = new DaNode(string.Empty, string.Empty);
            RootNode = root;
            AddNodeToCache(root);
        }

        /// <summary>
        /// Gets the current status of the OPC Client
        /// </summary>
        public OpcStatus Status
        {
            get
            {
                if (_server == null || _server.GetServerStatus().ServerState != OpcServerState.Operational)
                    return OpcStatus.NotConnected;
                return OpcStatus.Connected;
            }
        }

        /// <summary>
        /// Read a tag
        /// </summary>
        /// <typeparam name="T">The type of tag to read</typeparam>
        /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
        /// E.g: the tag `foo.bar` reads the tag `bar` on the folder `foo`</param>
        /// <returns>The value retrieved from the OPC</returns>
        public ReadEvent<T> Read<T>(string tag)
        {
            var item = new OpcDa.TsCDaItem { ItemName = tag };
            if (Status == OpcStatus.NotConnected)
            {
                throw new OpcException("Server not connected. Cannot read tag.");
            }
            var result = _server.Read(new[] { item })[0];
            T casted;
            TryCastResult(result.Value, out casted);

            var readEvent = new ReadEvent<T>();
            readEvent.Value = casted;
            readEvent.SourceTimestamp = result.Timestamp;
            readEvent.ServerTimestamp = result.Timestamp;
            if (result.Quality == OpcDa.TsCDaQuality.Good) readEvent.Quality = Quality.Good;
            if (result.Quality == OpcDa.TsCDaQuality.Bad) readEvent.Quality = Quality.Bad;

            return readEvent;
        }

        /// <summary>
        /// Write a value on the specified opc tag
        /// </summary>
        /// <typeparam name="T">The type of tag to write on</typeparam>
        /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
        /// E.g: the tag `foo.bar` writes on the tag `bar` on the folder `foo`</param>
        /// <param name="item"></param>
        public void Write<T>(string tag, T item)
        {
            var itmVal = new OpcDa.TsCDaItemValue
            {
                ItemName = tag,
                Value = item
            };
            var result = _server.Write(new[] { itmVal })[0];
            CheckResult(result, tag);
        }

        /// <summary>
        /// Casts result of monitoring and reading values
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="casted">The casted result</param>
        /// <typeparam name="T">Type of object to try to cast</typeparam>
        public void TryCastResult<T>(object value, out T casted)
        {
            try
            {
                casted = (T)value;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException(
                  string.Format(
                    "Could not monitor tag. Cast failed for type \"{0}\" on the new value \"{1}\" with type \"{2}\". Make sure tag data type matches.",
                    typeof(T), value, value.GetType()));
            }
        }

        /// <summary>
        /// Monitor the specified tag for changes
        /// </summary>
        /// <typeparam name="T">the type of tag to monitor</typeparam>
        /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
        /// E.g: the tag `foo.bar` monitors the tag `bar` on the folder `foo`</param>
        /// <param name="callback">the callback to execute when the value is changed.
        /// The first parameter is a MonitorEvent object which represents the data point, the second is an `unsubscribe` function to unsubscribe the callback</param>
        public void Monitor<T>(string tag, Action<ReadEvent<T>, Action> callback)
        {
            var subItem = new OpcDa.TsCDaSubscriptionState
            {
                Name = (++_sub).ToString(CultureInfo.InvariantCulture),
                Active = true,
                UpdateRate = DefaultMonitorInterval
            };
            var sub = _server.CreateSubscription(subItem);

            // I have to start a new thread here because unsubscribing
            // the subscription during a datachanged event causes a deadlock
            Action unsubscribe = () => new Thread(o =>
              _server.CancelSubscription(sub)).Start();

            sub.DataChangedEvent += (handle, requestHandle, values) =>
            {
                T casted;
                TryCastResult(values[0].Value, out casted);
                var monitorEvent = new ReadEvent<T>();
                monitorEvent.Value = casted;
                monitorEvent.SourceTimestamp = values[0].Timestamp;
                monitorEvent.ServerTimestamp = values[0].Timestamp;
                if (values[0].Quality == OpcDa.TsCDaQuality.Good) monitorEvent.Quality = Quality.Good;
                if (values[0].Quality == OpcDa.TsCDaQuality.Bad) monitorEvent.Quality = Quality.Bad;
                callback(monitorEvent, unsubscribe);
            };
            sub.AddItems(new[] { new OpcDa.TsCDaItem { ItemName = tag } });
            sub.SetEnabled(true);
        }

        /// <summary>
        /// Finds a node on the Opc Server
        /// </summary>
        /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
        /// E.g: the tag `foo.bar` finds the tag `bar` on the folder `foo`</param>
        /// <returns>If there is a tag, it returns it, otherwise it throws an </returns>
        public DaNode FindNode(string tag)
        {
            // if the tag already exists in cache, return it
            if (_nodesCache.ContainsKey(tag))
                return _nodesCache[tag];

            // try to find the tag otherwise
            var item = new OpcDa.TsCDaItem { ItemName = tag };
            OpcDa.TsCDaItemValueResult result;
            try
            {
                result = _server.Read(new[] { item })[0];
            }
            catch (NullReferenceException)
            {
                throw new OpcException("Could not find node because server not connected.");
            }
            CheckResult(result, tag);
            var node = new DaNode(item.ItemName, item.ItemName, RootNode);
            AddNodeToCache(node);
            return node;
        }

        /// <summary>
        /// Gets the root node of the server
        /// </summary>
        public DaNode RootNode { get; private set; }

        /// <summary>
        /// Explore a folder on the Opc Server
        /// </summary>
        /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
        /// E.g: the tag `foo.bar` finds the sub nodes of `bar` on the folder `foo`</param>
        /// <returns>The list of sub-nodes</returns>
        public IEnumerable<DaNode> ExploreFolder(string tag)
        {
            var parent = FindNode(tag);
            OpcDa.TsCDaBrowsePosition p;
            var nodes = _server.Browse(new OpcItem(parent.Tag), new OpcDa.TsCDaBrowseFilters(), out p)
              .Select(t => new DaNode(t.Name, t.ItemName, parent))
              .ToList();
            //add nodes to cache
            foreach (var node in nodes)
                AddNodeToCache(node);

            return nodes;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_server != null)
                _server.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Adds a node to the cache using the tag as its key
        /// </summary>
        /// <param name="node">the node to add</param>
        private void AddNodeToCache(DaNode node)
        {
            if (!_nodesCache.ContainsKey(node.Tag))
                _nodesCache.Add(node.Tag, node);
        }

        private static void CheckResult(IOpcResult result, string tag)
        {
            if (result == null)
                throw new OpcException("The server replied with an empty response");
            if (result.Result.ToString() != "S_OK" && result.Result.ToString() != "E_READONLY")
                throw new OpcException(string.Format("Invalid response from the server. (Response Status: {0}, Opc Tag: {1})", result.Result, tag));
        }
    }
}

