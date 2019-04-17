using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public interface IResourceProvider
    {
        /// <summary>
        /// id for the resource provider
        /// </summary>
        string Id { get; }

        event ResourceChangedEventHandler Changed;

        /// <summary>
        /// enumerate resources
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        IEnumerable<IResource> GetResources(string extension);
    }
}
