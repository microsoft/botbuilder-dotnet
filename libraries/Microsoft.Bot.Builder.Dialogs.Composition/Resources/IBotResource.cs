using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Resources
{
    public interface IBotResource
    {
        /// <summary>
        /// The source this resource came from
        /// </summary>
        IBotResourceProvider Source { get; set; }

        /// <summary>
        /// Unique id of the resource
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// name of the resource
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// ContentType (cog, lg, etc.)
        /// </summary>
        string ResourceType { get; set; }

        /// <summary>
        /// Load the resource as string
        /// </summary>
        /// <returns></returns>
        Task<string> GetTextAsync();

        /// <summary>
        /// Load the resource as binary
        /// </summary>
        /// <returns></returns>
        Task<byte[]> GetBinaryAsync();
    }
}
