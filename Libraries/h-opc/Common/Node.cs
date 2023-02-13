using System.Collections.Generic;

namespace Hylasoft.Opc.Common
{
  /// <summary>
  /// Base class representing a node on the OPC server
  /// </summary>
  public abstract class Node
  {
    /// <summary>
    /// Gets the displayed name of the node
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets the dot-separated fully qualified tag of the node
    /// </summary>
    public string Tag { get; protected set; }

    /// <summary>
    /// Gets the parent node. If the node is root, returns null
    /// </summary>
    public Node Parent { get; private set; }

    /// <summary>
    /// Creates a new node
    /// </summary>
    /// <param name="name">the name of the node</param>
    /// <param name="parent">The parent node</param>
    protected Node(string name, Node parent = null)
    {
      Name = name;
      Parent = parent;
      if (parent != null && !string.IsNullOrEmpty(parent.Tag))
        Tag = parent.Tag + '.' + name;
      else
        Tag = name;
    }

    /// <summary>
    /// Overrides ToString()
    /// </summary>
    public override string ToString()
    {
      return Tag;
    }
  }
}
