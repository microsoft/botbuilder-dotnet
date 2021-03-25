// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Runtime.Tests.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Runtime.Tests
{
    public class TestDataGenerator
    {
        public static ResourceExplorer BuildMemoryResourceExplorer(IEnumerable<MemoryResource> resources = null)
        {
            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MemoryResourceProvider(
                resourceExplorer,
                resources ?? Array.Empty<MemoryResource>());

            resourceExplorer.AddResourceProvider(resourceProvider);

            return resourceExplorer;
        }
    }
}
