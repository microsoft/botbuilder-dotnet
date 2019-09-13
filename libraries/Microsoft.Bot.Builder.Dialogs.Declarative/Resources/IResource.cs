using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public interface IResource
    {
        /// <summary>
        /// Gets resource name.
        /// </summary>
        /// <value>
        /// Resource name.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Get resource as text async.
        /// </summary>
        /// <returns>The resource as text.</returns>
        Task<string> ReadTextAsync();

        /// <summary>
        /// Get readonly stream. 
        /// </summary>
        /// <returns>The resource as a stream.</returns>
        Task<Stream> OpenStreamAsync();
    }
}
