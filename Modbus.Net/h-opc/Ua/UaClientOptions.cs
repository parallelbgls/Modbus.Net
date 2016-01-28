using System;
using System.Security.Cryptography.X509Certificates;

namespace Hylasoft.Opc.Ua
{
  /// <summary>
  /// This class defines the configuration options for the setup of the UA client session
  /// </summary>
  public class UaClientOptions
  {
    /// <summary>
    /// Specifies the (optional) certificate for the applicaiton to connect to the server
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


    internal UaClientOptions()
    {
      // Initialize default values:
      ApplicationName = "h-opc-client";
      AutoAcceptUntrustedCertificates = true;
      ConfigSectionName = "h-opc-client";
      DefaultMonitorInterval = 100;
      SessionName = "h-opc-client";
      SessionTimeout = 60000U;
      UseMessageSecurity = false;
    }
  }
}