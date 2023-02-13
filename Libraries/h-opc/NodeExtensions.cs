using Hylasoft.Opc.Common;
using OpcF = Opc.Ua;

namespace Hylasoft.Opc.Ua
{
  /// <summary>
  /// Class with extension methods for OPC UA
  /// </summary>
  public static class NodeExtensions
  {
    /// <summary>
    /// Converts an OPC Foundation node to an Hylasoft OPC UA Node
    /// </summary>
    /// <param name="node">The node to convert</param>
    /// <param name="parent">the parent node (optional)</param>
    /// <returns></returns>
    internal static UaNode ToHylaNode(this OpcF.ReferenceDescription node, Node parent = null)
    {
      var name = node.DisplayName.ToString();
      var nodeId = node.NodeId.ToString();
      return new UaNode(name, nodeId, parent);
    }
  }
}