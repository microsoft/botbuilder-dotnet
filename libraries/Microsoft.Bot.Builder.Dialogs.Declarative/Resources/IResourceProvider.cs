using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public interface IResourceProvider
    {
        /// <summary>
        /// Gets id for the resource provider.
        /// </summary>
        /// <value>
        /// id for the resource provider.
        /// </value>
        string Id { get; }

        event ResourceChangedEventHandler Changed;

        /// <summary>
        /// Get resource by id.
        /// </summary>
        /// <param name="id">resource id.</param>
        /// <returns>resource.</returns>
        IResource GetResource(string id);

        /// <summary>
        /// enumerate resources.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        IEnumerable<IResource> GetResources(string extension);
    }
}
