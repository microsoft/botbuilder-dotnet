// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Interface for looking up a resource by id.
    /// </summary>
    public abstract class ResourceProvider
    {
        public ResourceProvider(ResourceExplorer resourceExplorer)
        {
            this.ResourceExplorer = resourceExplorer;
        }

        /// <summary>
        /// Event which is fired if any resource managed by the resource provider detects changes to the underlining resource.
        /// </summary>
        public event EventHandler<IEnumerable<Resource>> Changed;

        /// <summary>
        /// Gets the  resource Explorer.
        /// </summary>
        /// <value>
        /// Resource Explorer.
        /// </value>
        public ResourceExplorer ResourceExplorer { get; private set; }

        /// <summary>
        /// Gets or sets id for the resource provider.
        /// </summary>
        /// <value>
        /// id for the resource provider.
        /// </value>
        public string Id { get; protected set; }

        /// <summary>
        /// Get resource by id.
        /// </summary>
        /// <param name="id">Resource id.</param>
        /// <param name="resource">resource.</param>
        /// <returns>true if resource is found.</returns>
        public abstract bool TryGetResource(string id, out Resource resource);

        /// <summary>
        /// enumerate resources.
        /// </summary>
        /// <param name="extension">Extension filter.</param>
        /// <returns>The resources.</returns>
        public abstract IEnumerable<Resource> GetResources(string extension);

        /// <summary>
        /// Refresh any cached resources.
        /// </summary>
        public abstract void Refresh();

        protected virtual void OnChanged(IEnumerable<Resource> resources)
        {
            Changed?.Invoke(this, resources);
        }
    }
}
