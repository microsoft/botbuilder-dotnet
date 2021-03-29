// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.Runtime;
using Microsoft.Bot.Builder.Integration.Runtime.Settings;
using Microsoft.Bot.Builder.Runtime.Tests.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var memoryFileProvider = new InMemoryFileProvider(JsonConvert.SerializeObject(new { runtimeSettings = runtimeSettings }));
            return builder.AddJsonFile(memoryFileProvider, "appsettings.json", false, false);
        }

        private class InMemoryFileProvider : IFileProvider
        {
            private readonly IFileInfo _fileInfo;

            public InMemoryFileProvider(string json) => _fileInfo = new InMemoryFile(json);

            public IFileInfo GetFileInfo(string f) => _fileInfo;

            public IDirectoryContents GetDirectoryContents(string f) => null;

            public IChangeToken Watch(string f) => NullChangeToken.Singleton;

            private class InMemoryFile : IFileInfo
            {
                private readonly byte[] _data;

                public InMemoryFile(string json) => _data = Encoding.UTF8.GetBytes(json);

                public bool Exists { get; } = true;

                public long Length => _data.Length;

                public string PhysicalPath { get; } = string.Empty;

                public string Name { get; } = string.Empty;

                public DateTimeOffset LastModified { get; } = DateTimeOffset.UtcNow;

                public bool IsDirectory { get; } = false;

                public Stream CreateReadStream() => new MemoryStream(_data);
            }
        }
    }
}
