using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hylasoft.Opc.Common
{
  /// <summary>
  /// Base class representing a monitor event on the OPC server
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ReadEvent<T>
  {
    /// <summary>
    /// Gets the value that was read from the server
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Gets the quality of the signal from the server
    /// </summary>
    [DefaultValue(Common.Quality.Unknown)]
    public Quality Quality { get; set; }

    /// <summary>
    /// Gets the source timestamp on when the event ocurred
    /// </summary>
    public DateTime SourceTimestamp { get; set; }

    /// <summary>
    /// Gets the server timestamp on when the event ocurred
    /// </summary>
    public DateTime ServerTimestamp { get; set; }
  }
}
