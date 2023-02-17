using Hylasoft.Opc.Common;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hylasoft.Opc.Ua
{
  /// <summary>
  /// Client Implementation for UA
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
    Justification = "Doesn't make sense to split this class")]
  public class UaClient : IClient<UaNode>
  {
    private readonly UaClientOptions _options = new UaClientOptions();
    private readonly Uri _serverUrl;
    private Session _session;

    private readonly IDictionary<string, UaNode> _nodesCache = new Dictionary<string, UaNode>();
    private readonly IDictionary<string, IList<UaNode>> _folderCache = new Dictionary<string, IList<UaNode>>();

    /// <summary>
    /// Creates a server object
    /// </summary>
    /// <param name="serverUrl">the url of the server to connect to</param>
    public UaClient(Uri serverUrl)
    {
      _serverUrl = serverUrl;
      Status = OpcStatus.NotConnected;
    }

    /// <summary>
    /// Creates a server object
    /// </summary>
    /// <param name="serverUrl">the url of the server to connect to</param>
    /// <param name="options">custom options to use with ua client</param>
    public UaClient(Uri serverUrl, UaClientOptions options)
    {
      _serverUrl = serverUrl;
      _options = options;
      Status = OpcStatus.NotConnected;
    }

    /// <summary>
    /// Options to configure the UA client session
    /// </summary>
    public UaClientOptions Options
    {
      get { return _options; }
    }

    /// <summary>
    /// OPC Foundation underlying session object
    /// </summary>
    protected Session Session
    {
      get
      {
        return _session;
      }
    }

    private void PostInitializeSession()
    {
      var node = _session.NodeCache.Find(ObjectIds.ObjectsFolder);
      RootNode = new UaNode(string.Empty, node.NodeId.ToString());
      AddNodeToCache(RootNode);
      Status = OpcStatus.Connected;
    }

    /// <summary>
    /// Connect the client to the OPC Server
    /// </summary>
    public async Task Connect()
    {
      if (Status == OpcStatus.Connected)
        return;
      _session = await InitializeSession(_serverUrl);
      _session.KeepAlive += SessionKeepAlive;
      _session.SessionClosing += SessionClosing;
      PostInitializeSession();
    }

    /// <summary>
    /// Gets the datatype of an OPC tag
    /// </summary>
    /// <param name="tag">Tag to get datatype of</param>
    /// <returns>System Type</returns>
    public System.Type GetDataType(string tag)
    {
      var nodesToRead = BuildReadValueIdCollection(tag, Attributes.Value);
      DataValueCollection results;
      DiagnosticInfoCollection diag;
      _session.Read(
          requestHeader: null,
          maxAge: 0,
          timestampsToReturn: TimestampsToReturn.Neither,
          nodesToRead: nodesToRead,
          results: out results,
          diagnosticInfos: out diag);
      var type = results[0].WrappedValue.TypeInfo.BuiltInType;
      return System.Type.GetType("System." + type.ToString());
    }

    private void SessionKeepAlive(ISession session, KeepAliveEventArgs e)
    {
      if (e.CurrentState != ServerState.Running)
      {
        if (Status == OpcStatus.Connected)
        {
          Status = OpcStatus.NotConnected;
          NotifyServerConnectionLost();
        }
      }
      else if (e.CurrentState == ServerState.Running)
      {
        if (Status == OpcStatus.NotConnected)
        {
          Status = OpcStatus.Connected;
          NotifyServerConnectionRestored();
        }
      }
    }

    private void SessionClosing(object? sender, EventArgs e)
    {
      Status = OpcStatus.NotConnected;
      NotifyServerConnectionLost();
    }


    /// <summary>
    /// Reconnect the OPC session
    /// </summary>
    public void ReConnect()
    {
      Status = OpcStatus.NotConnected;
      _session.Reconnect();
      Status = OpcStatus.Connected;
    }

    /// <summary>
    /// Create a new OPC session, based on the current session parameters.
    /// </summary>
    public void RecreateSession()
    {
      Status = OpcStatus.NotConnected;
      _session = Session.Recreate(_session);
      PostInitializeSession();
    }


    /// <summary>
    /// Gets the current status of the OPC Client
    /// </summary>
    public OpcStatus Status { get; private set; }


    private ReadValueIdCollection BuildReadValueIdCollection(string tag, uint attributeId)
    {
      var n = FindNode(tag, RootNode);
      var readValue = new ReadValueId
      {
        NodeId = n.NodeId,
        AttributeId = attributeId
      };
      return new ReadValueIdCollection { readValue };
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
      var nodesToRead = BuildReadValueIdCollection(tag, Attributes.Value);
      DataValueCollection results;
      DiagnosticInfoCollection diag;
      _session.Read(
          requestHeader: null,
          maxAge: 0,
          timestampsToReturn: TimestampsToReturn.Neither,
          nodesToRead: nodesToRead,
          results: out results,
          diagnosticInfos: out diag);
      var val = results[0];

      var readEvent = new ReadEvent<T>();
      readEvent.Value = (T)val.Value;
      readEvent.SourceTimestamp = val.SourceTimestamp;
      readEvent.ServerTimestamp = val.ServerTimestamp;
      if (StatusCode.IsGood(val.StatusCode)) readEvent.Quality = Quality.Good;
      if (StatusCode.IsBad(val.StatusCode)) readEvent.Quality = Quality.Bad;
      return readEvent;
    }


    /// <summary>
    /// Read a tag asynchronously
    /// </summary>
    /// <typeparam name="T">The type of tag to read</typeparam>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` reads the tag `bar` on the folder `foo`</param>
    /// <returns>The value retrieved from the OPC</returns>
    public Task<ReadEvent<T>> ReadAsync<T>(string tag)
    {
      var nodesToRead = BuildReadValueIdCollection(tag, Attributes.Value);

      // Wrap the ReadAsync logic in a TaskCompletionSource, so we can use C# async/await syntax to call it:
      var taskCompletionSource = new TaskCompletionSource<ReadEvent<T>>();
      _session.BeginRead(
          requestHeader: null,
          maxAge: 0,
          timestampsToReturn: TimestampsToReturn.Neither,
          nodesToRead: nodesToRead,
          callback: ar =>
          {
            DataValueCollection results;
            DiagnosticInfoCollection diag;
            var response = _session.EndRead(
                result: ar,
                results: out results,
                diagnosticInfos: out diag);

            try
            {
              CheckReturnValue(response.ServiceResult);
              var val = results[0];
              var readEvent = new ReadEvent<T>();
              readEvent.Value = (T)val.Value;
              readEvent.SourceTimestamp = val.SourceTimestamp;
              readEvent.ServerTimestamp = val.ServerTimestamp;
              if (StatusCode.IsGood(val.StatusCode)) readEvent.Quality = Quality.Good;
              if (StatusCode.IsBad(val.StatusCode)) readEvent.Quality = Quality.Bad;
              taskCompletionSource.TrySetResult(readEvent);
            }
            catch (Exception ex)
            {
              taskCompletionSource.TrySetException(ex);
            }
          },
          asyncState: null);

      return taskCompletionSource.Task;
    }


    private WriteValueCollection BuildWriteValueCollection(string tag, uint attributeId, object dataValue)
    {
      var n = FindNode(tag, RootNode);
      var writeValue = new WriteValue
      {
        NodeId = n.NodeId,
        AttributeId = attributeId,
        Value = { Value = dataValue }
      };
      return new WriteValueCollection { writeValue };
    }

    /// <summary>
    /// Write a value on the specified opc tag
    /// </summary>
    /// <typeparam name="T">The type of tag to write on</typeparam>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` writes on the tag `bar` on the folder `foo`</param>
    /// <param name="item">The value for the item to write</param>
    public void Write<T>(string tag, T item)
    {
      var nodesToWrite = BuildWriteValueCollection(tag, Attributes.Value, item);

      StatusCodeCollection results;
      DiagnosticInfoCollection diag;
      _session.Write(
          requestHeader: null,
          nodesToWrite: nodesToWrite,
          results: out results,
          diagnosticInfos: out diag);

      CheckReturnValue(results[0]);
    }

    /// <summary>
    /// Write a value on the specified opc tag asynchronously
    /// </summary>
    /// <typeparam name="T">The type of tag to write on</typeparam>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` writes on the tag `bar` on the folder `foo`</param>
    /// <param name="item">The value for the item to write</param>
    public Task WriteAsync<T>(string tag, T item)
    {
      var nodesToWrite = BuildWriteValueCollection(tag, Attributes.Value, item);

      // Wrap the WriteAsync logic in a TaskCompletionSource, so we can use C# async/await syntax to call it:
      var taskCompletionSource = new TaskCompletionSource<StatusCode>();
      _session.BeginWrite(
          requestHeader: null,
          nodesToWrite: nodesToWrite,
          callback: ar =>
          {
            StatusCodeCollection results;
            DiagnosticInfoCollection diag;
            var response = _session.EndWrite(
                result: ar,
                results: out results,
                diagnosticInfos: out diag);
            try
            {
              CheckReturnValue(response.ServiceResult);
              CheckReturnValue(results[0]);
              taskCompletionSource.SetResult(response.ServiceResult);
            }
            catch (Exception ex)
            {
              taskCompletionSource.TrySetException(ex);
            }
          },
          asyncState: null);
      return taskCompletionSource.Task;
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
      var node = FindNode(tag);

      var sub = new Subscription
      {
        PublishingInterval = _options.DefaultMonitorInterval,
        PublishingEnabled = true,
        LifetimeCount = _options.SubscriptionLifetimeCount,
        KeepAliveCount = _options.SubscriptionKeepAliveCount,
        DisplayName = tag,
        Priority = byte.MaxValue
      };

      var item = new MonitoredItem
      {
        StartNodeId = node.NodeId,
        AttributeId = Attributes.Value,
        DisplayName = tag,
        SamplingInterval = _options.DefaultMonitorInterval
      };
      sub.AddItem(item);
      _session.AddSubscription(sub);
      sub.Create();
      sub.ApplyChanges();

      item.Notification += (monitoredItem, args) =>
      {
        var p = (MonitoredItemNotification)args.NotificationValue;
        var t = p.Value.WrappedValue.Value;
        Action unsubscribe = () =>
        {
          sub.RemoveItems(sub.MonitoredItems);
          sub.Delete(true);
          _session.RemoveSubscription(sub);
          sub.Dispose();
        };

        var monitorEvent = new ReadEvent<T>();
        monitorEvent.Value = (T)t;
        monitorEvent.SourceTimestamp = p.Value.SourceTimestamp;
        monitorEvent.ServerTimestamp = p.Value.ServerTimestamp;
        if (StatusCode.IsGood(p.Value.StatusCode)) monitorEvent.Quality = Quality.Good;
        if (StatusCode.IsBad(p.Value.StatusCode)) monitorEvent.Quality = Quality.Bad;
        callback(monitorEvent, unsubscribe);
      };
    }

    /// <summary>
    /// Explore a folder on the Opc Server
    /// </summary>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` finds the sub nodes of `bar` on the folder `foo`</param>
    /// <returns>The list of sub-nodes</returns>
    public IEnumerable<UaNode> ExploreFolder(string tag)
    {
      IList<UaNode> nodes;
      _folderCache.TryGetValue(tag, out nodes);
      if (nodes != null)
        return nodes;

      var folder = FindNode(tag);
      nodes = ClientUtils.Browse(_session, folder.NodeId)
        .GroupBy(n => n.NodeId) //this is to select distinct
        .Select(n => n.First())
        .Where(n => n.NodeClass == NodeClass.Variable || n.NodeClass == NodeClass.Object)
        .Select(n => n.ToHylaNode(folder))
        .ToList();

      //add nodes to cache
      if (!_folderCache.ContainsKey(tag))
        _folderCache.Add(tag, nodes);
      foreach (var node in nodes)
        AddNodeToCache(node);

      return nodes;
    }

    /// <summary>
    /// Explores a folder asynchronously
    /// </summary>
    public async Task<IEnumerable<Common.Node>> ExploreFolderAsync(string tag)
    {
      return await Task.Run(() => ExploreFolder(tag));
    }

    /// <summary>
    /// Finds a node on the Opc Server
    /// </summary>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` finds the tag `bar` on the folder `foo`</param>
    /// <returns>If there is a tag, it returns it, otherwise it throws an </returns>
    public UaNode FindNode(string tag)
    {
      // if the tag already exists in cache, return it
      if (_nodesCache.ContainsKey(tag))
        return _nodesCache[tag];

      // try to find the tag otherwise
      var found = FindNode(tag, RootNode);
      if (found != null)
      {
        AddNodeToCache(found);
        return found;
      }

      // throws an exception if not found
      throw new OpcException(string.Format("The tag \"{0}\" doesn't exist on the Server", tag));
    }

    /// <summary>
    /// Find node asynchronously
    /// </summary>
    public async Task<Common.Node> FindNodeAsync(string tag)
    {
      return await Task.Run(() => FindNode(tag));
    }

    /// <summary>
    /// Gets the root node of the server
    /// </summary>
    public UaNode RootNode { get; private set; }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (_session != null)
      {
        _session.RemoveSubscriptions(_session.Subscriptions.ToList());
        _session.Close();
        _session.Dispose();
      }
      GC.SuppressFinalize(this);
    }

    private void CheckReturnValue(StatusCode status)
    {
      if (!StatusCode.IsGood(status))
        throw new OpcException(string.Format("Invalid response from the server. (Response Status: {0})", status), status);
    }

    /// <summary>
    /// Adds a node to the cache using the tag as its key
    /// </summary>
    /// <param name="node">the node to add</param>
    private void AddNodeToCache(UaNode node)
    {
      if (!_nodesCache.ContainsKey(node.Tag))
        _nodesCache.Add(node.Tag, node);
    }

    /// <summary>
    /// Return identity login object for a given URI.
    /// </summary>
    /// <param name="url">Login URI</param>
    /// <returns>AnonUser or User with name and password</returns>
    private UserIdentity GetIdentity(Uri url)
    {
      if (_options.UserIdentity != null)
      {
        return _options.UserIdentity;
      }
      var uriLogin = new UserIdentity();
      if (!string.IsNullOrEmpty(url.UserInfo))
      {
        var uis = url.UserInfo.Split(':');
        uriLogin = new UserIdentity(uis[0], uis[1]);
      }
      return uriLogin;
    }

    /// <summary>
    /// Crappy method to initialize the session. I don't know what many of these things do, sincerely.
    /// </summary>
    private async Task<Session> InitializeSession(Uri url)
    {
      var certificateValidator = new CertificateValidator();
      certificateValidator.CertificateValidation += (sender, eventArgs) =>
      {
        if (ServiceResult.IsGood(eventArgs.Error))
          eventArgs.Accept = true;
        else if ((eventArgs.Error.StatusCode.Code == StatusCodes.BadCertificateUntrusted) && _options.AutoAcceptUntrustedCertificates)
          eventArgs.Accept = true;
        else
          throw new OpcException(string.Format("Failed to validate certificate with error code {0}: {1}", eventArgs.Error.Code, eventArgs.Error.AdditionalInfo), eventArgs.Error.StatusCode);
      };
      // Build the application configuration
      var appInstance = new ApplicationInstance
      {
        ApplicationType = ApplicationType.Client,
        ConfigSectionName = _options.ConfigSectionName,
        ApplicationConfiguration = new ApplicationConfiguration
        {
          ApplicationUri = url.ToString(),
          ApplicationName = _options.ApplicationName,
          ApplicationType = ApplicationType.Client,
          CertificateValidator = certificateValidator,
          ServerConfiguration = new ServerConfiguration
          {
            MaxSubscriptionCount = _options.MaxSubscriptionCount,
            MaxMessageQueueSize = _options.MaxMessageQueueSize,
            MaxNotificationQueueSize = _options.MaxNotificationQueueSize,
            MaxPublishRequestCount = _options.MaxPublishRequestCount
          },
          SecurityConfiguration = new SecurityConfiguration
          {
            AutoAcceptUntrustedCertificates = _options.AutoAcceptUntrustedCertificates
          },
          TransportQuotas = new TransportQuotas
          {
            OperationTimeout = 600000,
            MaxStringLength = 1048576,
            MaxByteStringLength = 1048576,
            MaxArrayLength = 65535,
            MaxMessageSize = 4194304,
            MaxBufferSize = 65535,
            ChannelLifetime = 600000,
            SecurityTokenLifetime = 3600000
          },
          ClientConfiguration = new ClientConfiguration
          {
            DefaultSessionTimeout = 60000,
            MinSubscriptionLifetime = 10000
          },
          DisableHiResClock = true
        }
      };

      // Assign a application certificate (when specified)
      if (_options.ApplicationCertificate != null)
        appInstance.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate = new CertificateIdentifier(_options.ApplicationCertificate);

      // Find the endpoint to be used
      var endpoints = ClientUtils.SelectEndpoint(url, _options.UseMessageSecurity);

      // Create the OPC session:
      var session = await Session.Create(
          configuration: appInstance.ApplicationConfiguration,
          endpoint: new ConfiguredEndpoint(
              collection: null,
              description: endpoints,
              configuration: EndpointConfiguration.Create(applicationConfiguration: appInstance.ApplicationConfiguration)),
          updateBeforeConnect: false,
          checkDomain: false,
          sessionName: _options.SessionName,
          sessionTimeout: _options.SessionTimeout,
          identity: GetIdentity(url),
          preferredLocales: new string[] { });

      return session;
    }

    /// <summary>
    /// Finds a node starting from the specified node as the root folder
    /// </summary>
    /// <param name="tag">the tag to find</param>
    /// <param name="node">the root node</param>
    /// <returns></returns>
    private UaNode FindNode(string tag, UaNode node)
    {
      var folders = tag.Split('.');
      var head = folders.FirstOrDefault();
      UaNode found;
      try
      {
        var subNodes = ExploreFolder(node.Tag);
        found = subNodes.Single(n => n.Name == head);
      }
      catch (Exception ex)
      {
        throw new OpcException(string.Format("The tag \"{0}\" doesn't exist on folder \"{1}\"", head, node.Tag), ex);
      }

      // remove an array element by converting it to a list
      var folderList = folders.ToList();
      folderList.RemoveAt(0); // remove the first node
      folders = folderList.ToArray();
      return folders.Length == 0
        ? found // last node, return it
        : FindNode(string.Join(".", folders), found); // find sub nodes
    }


    private void NotifyServerConnectionLost()
    {
      if (ServerConnectionLost != null)
        ServerConnectionLost(this, EventArgs.Empty);
    }

    private void NotifyServerConnectionRestored()
    {
      if (ServerConnectionRestored != null)
        ServerConnectionRestored(this, EventArgs.Empty);
    }

    /// <summary>
    /// This event is raised when the connection to the OPC server is lost.
    /// </summary>
    public event EventHandler ServerConnectionLost;

    /// <summary>
    /// This event is raised when the connection to the OPC server is restored.
    /// </summary>
    public event EventHandler ServerConnectionRestored;

  }

}
