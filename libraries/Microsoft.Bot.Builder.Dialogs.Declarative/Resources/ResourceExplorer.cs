// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public delegate void ResourceChangedEventHandler(IResource[] resources);

    /// <summary>
    /// Class which gives standard access to file based resources.
    /// </summary>
    public class ResourceExplorer : IDisposable
    {
        private List<IResourceProvider> resourceProviders = new List<IResourceProvider>();

        private CancellationTokenSource cancelReloadToken = new CancellationTokenSource();
        private ConcurrentBag<IResource> changedResources = new ConcurrentBag<IResource>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceExplorer"/> class.
        /// </summary>
        public ResourceExplorer()
        {
        }

        /// <summary>
        /// Event which fires when a resource is changed.
        /// </summary>
        public event ResourceChangedEventHandler Changed;

        /// <summary>
        /// Gets the resource providers.
        /// </summary>
        /// <value>
        /// The resource providers.
        /// </value>
        public IEnumerable<IResourceProvider> ResourceProviders
        {
            get { return this.resourceProviders; }
        }

        /// <summary>
        /// Add a resource provider to the resources managed by the resource explorer.
        /// </summary>
        /// <param name="resourceProvider">resource provider.</param>
        /// <returns>resource explorer so that you can fluently call multiple methods on the resource explorer.</returns>
        public ResourceExplorer AddResourceProvider(IResourceProvider resourceProvider)
        {
            resourceProvider.Changed += ResourceProvider_Changed;

            if (this.resourceProviders.Any(r => r.Id == resourceProvider.Id))
            {
                throw new ArgumentException($"{resourceProvider.Id} has already been added as a resource");
            }

            this.resourceProviders.Add(resourceProvider);
            return this;
        }

        /// <summary>
        /// Create Type from resource.
        /// </summary>
        /// <typeparam name="T">type to create.</typeparam>
        /// <param name="resourceId">resourceId to bind to.</param>
        /// <returns>created type.</returns>
        public T LoadType<T>(string resourceId)
        {
            return this.LoadType<T>(this.GetResource(resourceId));
        }

        /// <summary>
        /// Create Type from resource.
        /// </summary>
        /// <typeparam name="T">type to create.</typeparam>
        /// <param name="resource">resource to bind to.</param>
        /// <returns>created type.</returns>
        public T LoadType<T>(IResource resource)
        {
            return DeclarativeTypeLoader.Load<T>(resource, this, DebugSupport.SourceMap);
        }

        /// <summary>
        /// Get resources of a given type.
        /// </summary>
        /// <param name="fileExtension">File extension filter.</param>
        /// <returns>The resources.</returns>
        public IEnumerable<IResource> GetResources(string fileExtension)
        {
            foreach (var resourceProvider in this.resourceProviders)
            {
                foreach (var resource in resourceProvider.GetResources(fileExtension))
                {
                    yield return resource;
                }
            }
        }

        /// <summary>
        /// Get resource by id.
        /// </summary>
        /// <param name="id">The resource id.</param>
        /// <returns>The resource, or throws if not found.</returns>
        public IResource GetResource(string id)
        {
            if (TryGetResource(id, out var resource))
            {
                return resource;
            }

            throw new ArgumentException($"Could not find resource '{id}'", paramName: id);
        }

        /// <summary>
        /// Try to get the resource by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="resource">resource that was found or null.</param>
        /// <returns>true if found.</returns>
        public bool TryGetResource(string id, out IResource resource)
        {
            foreach (var resourceProvider in this.resourceProviders)
            {
                if (resourceProvider.TryGetResource(id, out resource))
                {
                    return true;
                }
            }

            resource = null;
            return false;
        }

        public void Dispose()
        {
            foreach (var resource in this.resourceProviders)
            {
                if (resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private void ResourceProvider_Changed(IResource[] resources)
        {
            if (this.Changed != null)
            {
                foreach (var resource in resources)
                {
                    changedResources.Add(resource);
                }

                lock (cancelReloadToken)
                {
                    cancelReloadToken.Cancel();
                    cancelReloadToken = new CancellationTokenSource();
                    Task.Delay(1000, cancelReloadToken.Token)
                        .ContinueWith(t =>
                        {
                            if (t.IsCanceled)
                            {
                                return;
                            }

                            var changed = changedResources.ToArray();
                            changedResources = new ConcurrentBag<IResource>();
                            this.Changed(changed);
                        }).ContinueWith(t => t.Status);
                }
            }
        }
    }
}
