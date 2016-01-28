using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hylasoft.Opc.Common
{
  /// <summary>
  /// Client interface to perform basic Opc tasks, like discovery, monitoring, reading/writing tags,
  /// </summary>
  public interface IClient<out TNode> : IDisposable
    where TNode : Node
  {
    /// <summary>
    /// Connect the client to the OPC Server
    /// </summary>
    void Connect();

    /// <summary>
    /// Gets the current status of the OPC Client
    /// </summary>
    OpcStatus Status { get; }

    /// <summary>
    /// Read a tag
    /// </summary>
    /// <typeparam name="T">The type of tag to read</typeparam>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` reads the tag `bar` on the folder `foo`</param>
    /// <returns>The value retrieved from the OPC</returns>
    T Read<T>(string tag);

    /// <summary>
    /// Write a value on the specified opc tag
    /// </summary>
    /// <typeparam name="T">The type of tag to write on</typeparam>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` writes on the tag `bar` on the folder `foo`</param>
    /// <param name="item"></param>
    void Write<T>(string tag, T item);

    /// <summary>
    /// Monitor the specified tag for changes
    /// </summary>
    /// <typeparam name="T">the type of tag to monitor</typeparam>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` monitors the tag `bar` on the folder `foo`</param>
    /// <param name="callback">the callback to execute when the value is changed.
    /// The first parameter is the new value of the node, the second is an `unsubscribe` function to unsubscribe the callback</param>
    void Monitor<T>(string tag, Action<T, Action> callback);

    /// <summary>
    /// Finds a node on the Opc Server
    /// </summary>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` finds the tag `bar` on the folder `foo`</param>
    /// <returns>If there is a tag, it returns it, otherwise it throws an </returns>
    TNode FindNode(string tag);

    /// <summary>
    /// Gets the root node of the server
    /// </summary>
    TNode RootNode { get; }

    /// <summary>
    /// Explore a folder on the Opc Server
    /// </summary>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
    /// E.g: the tag `foo.bar` finds the sub nodes of `bar` on the folder `foo`</param>
    /// <returns>The list of sub-nodes</returns>
    IEnumerable<TNode> ExploreFolder(string tag);

    /// <summary>
    /// Read a tag asynchronusly
    /// </summary>
    Task<T> ReadAsync<T>(string tag);

    /// <summary>
    /// Write a value on the specified opc tag asynchronously
    /// </summary>
    Task WriteAsync<T>(string tag, T item);

    /// <summary>
    /// Finds a node on the Opc Server asynchronously
    /// </summary>
    Task<Node> FindNodeAsync(string tag);

    /// <summary>
    /// Explore a folder on the Opc Server asynchronously
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
      Justification = "Task")]
    Task<IEnumerable<Node>> ExploreFolderAsync(string tag);
  }
}