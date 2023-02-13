using System;
using System.Security.Cryptography.X509Certificates;
using OpcUa = Opc.Ua;

namespace Hylasoft.Opc.Ua
{
  /// <summary>
  /// This class defines the configuration options for the setup of the UA client session
  /// </summary>
  public class UaClientOptions
  {
    /// <summary>
    /// Specifies the (optional) certificate for the application to connect to the server
    /// </summary>
    public X509Certificate2 ApplicationCertificate { get; set; }

    /// <summary>
    /// Specifies the ApplicationName for the client application.
    /// </summary>
    public string ApplicationName { get; set; }

    /// <summary>
    /// Should untrusted certificates be silently accepted by the client?
    /// </summary>
    public bool AutoAcceptUntrustedCertificates { get; set; }

    /// <summary>
    /// Specifies the ConfigSectionName for the client configuration.
    /// </summary>
    public string ConfigSectionName { get; set; }

    /// <summary>
    /// default monitor interval in Milliseconds.
    /// </summary>
    public int DefaultMonitorInterval { get; set; }

    /// <summary>
    /// Specifies a name to be associated with the created sessions.
    /// </summary>
    public string SessionName { get; set; }

    /// <summary>
    /// Specifies the timeout for the sessions.
    /// </summary>
    public uint SessionTimeout { get; set; }

    /// <summary>
    /// Specify whether message exchange should be secured.
    /// </summary>
    public bool UseMessageSecurity { get; set; }

    /// <summary>
    /// The maximum number of notifications per publish request.
    /// The client’s responsibility is to send PublishRequests to the server,
    /// in order to enable the server to send PublishResponses back.
    /// The PublishResponses are used to deliver the notifications: but if there
    /// are no PublishRequests, the server cannot send a notification to the client.
    /// The server will also verify that the client is alive by checking that
    /// new PublishRequests are received – LifeTimeCount defines the number of
    /// PublishingIntervals to wait for a new PublishRequest, before realizing
    /// that the client is no longer active.The Subscription is then removed from
    /// the server.
    /// </summary>
    public uint SubscriptionLifetimeCount { get; set; }

    /// <summary>
    /// If there is no data to send after the next PublishingInterval,
    /// the server will skip it. But KeepAlive defines how many intervals may be skipped,
    /// before an empty notification is sent anyway: to give the client a hint that
    /// the subscription is still alive in the server and that there just has not been
    /// any data arriving to the client.
    /// </summary>
    public uint SubscriptionKeepAliveCount { get; set; }

    /// <summary>
    /// Gets or sets the max subscription count.
    /// </summary>
    public int MaxSubscriptionCount { get; set; }

    /// <summary>
    /// The maximum number of messages saved in the queue for each subscription.
    /// </summary>
    public int MaxMessageQueueSize { get; set; }

    /// <summary>
    /// The maximum number of notificates saved in the queue for each monitored item.
    /// </summary>
    public int MaxNotificationQueueSize { get; set; }

    /// <summary>
    /// Gets or sets the max publish request count.
    /// </summary>
    public int MaxPublishRequestCount { get; set; }

    /// <summary>
    /// The identity to connect to the OPC server as
    /// </summary>
    public OpcUa.UserIdentity UserIdentity { get; set; }

    /// <summary>
    /// Creates a client options object
    /// </summary>
    public UaClientOptions()
    {
      // Initialize default values:
      ApplicationName = "h-opc-client";
      AutoAcceptUntrustedCertificates = true;
      ConfigSectionName = "h-opc-client";
      DefaultMonitorInterval = 100;
      SessionName = "h-opc-client";
      SessionTimeout = 60000U;
      UseMessageSecurity = false;
      SubscriptionLifetimeCount = 0;
      SubscriptionKeepAliveCount = 0;
      MaxSubscriptionCount = 100;
      MaxMessageQueueSize = 10;
      MaxNotificationQueueSize = 100;
      MaxPublishRequestCount = 20;
    }
  }
}
