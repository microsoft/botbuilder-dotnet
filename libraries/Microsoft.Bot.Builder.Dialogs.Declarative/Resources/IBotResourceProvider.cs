using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public delegate void ResourceChangeHandler(IBotResourceProvider provider, IBotResource resource);

    public interface IBotResourceProvider
    {
        /// <summary>
        /// Id of the source 
        /// </summary>
        string Id { get;  }

        /// <summary>
        /// The resources available from this source
        /// </summary>
        Task<IBotResource[]> GetResources(string resourceType);
    }
}
