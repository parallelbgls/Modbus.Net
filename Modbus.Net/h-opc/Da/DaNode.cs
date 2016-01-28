using Hylasoft.Opc.Common;

namespace Hylasoft.Opc.Da
{
  /// <summary>
  /// Represents a node to be used specifically for OPC DA
  /// </summary>
  public class DaNode : Node
  {
    /// <summary>
    /// Instantiates a DaNode class
    /// </summary>
    /// <param name="name">the name of the node</param>
    /// <param name="tag"></param>
    /// <param name="parent">The parent node</param>
    public DaNode(string name, string tag, Node parent = null)
      : base(name, parent)
    {
      Tag = tag;
    }
  }
}
