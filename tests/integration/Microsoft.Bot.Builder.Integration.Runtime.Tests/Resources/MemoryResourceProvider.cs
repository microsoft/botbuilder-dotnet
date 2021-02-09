// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Runtime.Tests.Resources
{
    public class MemoryResourceProvider : ResourceProvider
    {
        private readonly IDictionary<string, MemoryResource> resources;

        public MemoryResourceProvider(ResourceExplorer resourceExplorer)
            : this(resourceExplorer, resources: Array.Empty<MemoryResource>())
        {
        }

        public MemoryResourceProvider(ResourceExplorer resourceExplorer, IEnumerable<MemoryResource> resources)
            : base(resourceExplorer)
        {
            this.resources = new Dictionary<string, MemoryResource>(StringComparer.OrdinalIgnoreCase);

            if (resources != null)
            {
                foreach (MemoryResource resource in resources)
                {
                    this.resources[resource.Id] = resource;
                }
            }
        }

        public override IEnumerable<Resource> GetResources(string extension)
        {
            return this.resources.Values;
        }

        public override void Refresh()
        {
        }

        public void SetResource(MemoryResource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            this.resources[resource.Id] = resource;
            this.OnChanged(this.resources.Values);
        }

        public override bool TryGetResource(string id, out Resource resource)
        {
            bool result = this.resources.TryGetValue(id, out MemoryResource memoryResource);
            resource = memoryResource;
            return result;
        }
    }
}
