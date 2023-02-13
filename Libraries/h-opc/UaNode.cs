using Hylasoft.Opc.Common;

namespace Hylasoft.Opc.Ua
{
  /// <summary>
  /// Represents a node to be used specifically for OPC UA
  /// </summary>
  public class UaNode : Node
  {
    /// <summary>
    /// The UA Id of the node
    /// </summary>
    public string NodeId { get; private set; }

    /// <summary>
    /// Instantiates a UaNode class
    /// </summary>
    /// <param name="name">the name of the node</param>
    /// <param name="nodeId">The UA Id of the node</param>
    /// <param name="parent">The parent node</param>
    internal UaNode(string name, string nodeId, Node parent = null)
      : base(name, parent)
    {
      NodeId = nodeId;
    }

  }

}
