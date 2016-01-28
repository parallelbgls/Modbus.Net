namespace Hylasoft.Opc.Common
{
  /// <summary>
  /// Useful extension methods for OPC Clients
  /// </summary>
  public static class ClientExtensions
  {
    /// <summary>
    /// Reads a tag from the OPC. If for whatever reason the read fails (Tag doesn't exist, server not available) returns a default value
    /// </summary>
    /// <param name="client">the opc client to use for the read</param>
    /// <param name="tag">The fully qualified identifier of the tag</param>
    /// <param name="defaultValue">the default value to read if the read fails</param>
    /// <returns></returns>
    public static T ReadOrdefault<T>(this IClient<Node> client, string tag, T defaultValue = default(T))
    {
      try
      {
        return client.Read<T>(tag);
      }
      catch (OpcException)
      {
        return defaultValue;
      }
    }
  }
}