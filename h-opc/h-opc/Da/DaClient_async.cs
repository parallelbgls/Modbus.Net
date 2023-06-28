using Hylasoft.Opc.Common;

namespace Hylasoft.Opc.Da
{
    /// <summary>
    /// Client Implementation for DA
    /// </summary>
    public partial class DaClient
    {
        /// <summary>
        /// Read a tag asynchronusly
        /// </summary>
        public async Task<ReadEvent<T>> ReadAsync<T>(string tag)
        {
            return await Task.Run(() => Read<T>(tag));
        }

        /// <summary>
        /// Write a value on the specified opc tag asynchronously
        /// </summary>
        public async Task WriteAsync<T>(string tag, T item)
        {
            await Task.Run(() => Write(tag, item));
        }

        /// <summary>
        /// Finds a node on the Opc Server asynchronously
        /// </summary>
        public async Task<Node> FindNodeAsync(string tag)
        {
            return await Task.Run(() => FindNode(tag));
        }

        /// <summary>
        /// Explore a folder on the Opc Server asynchronously
        /// </summary>
        public async Task<IEnumerable<Node>> ExploreFolderAsync(string tag)
        {
            return await Task.Run(() => ExploreFolder(tag));
        }
    }
}
