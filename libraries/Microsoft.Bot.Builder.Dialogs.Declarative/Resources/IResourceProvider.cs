using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public interface IResourceProvider
    {
        event ResourceChangedEventHandler Changed;

        /// <summary>
        /// Gets id for the resource provider.
        /// </summary>
        /// <value>
        /// id for the resource provider.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Get resource by id.
        /// </summary>
        /// <param name="id">Resource id.</param>
        /// <param name="resource">resource</param>
        /// <returns>true if resource is found</returns>
        bool TryGetResource(string id, out IResource resource);

        /// <summary>
        /// enumerate resources.
        /// </summary>
        /// <param name="extension">Extension filter.</param>
        /// <returns>The resources.</returns>
        IEnumerable<IResource> GetResources(string extension);
    }
}
