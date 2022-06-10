// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Runtime.Tests.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Tests
{
    public static class TestDataGenerator
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

        internal static IConfigurationBuilder AddRuntimeSettings(this IConfigurationBuilder builder, RuntimeSettings runtimeSettings)
        {
            var serializeSettings = JsonConvert.SerializeObject(new { runtimeSettings });
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(serializeSettings));
            return builder.AddJsonStream(stream);
        }
    }
}
