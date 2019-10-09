// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Plugins
{
    public class FilePlugin : IPlugin
    {
        private readonly FileDependencyInfo info;

        public FilePlugin(FileDependencyInfo dependencyInfo)
        {
            this.info = dependencyInfo;
        }

        public string SchemaUri { get; private set; }

        public Type Type { get; private set; }

        public ICustomDeserializer Loader { get; private set; }

        public async Task Load()
        {
            // Load assembly for factory registration and custom loader if applicable
            var assembly = Assembly.LoadFrom(info.AssemblyPath);

            this.Type = assembly.GetTypes().FirstOrDefault(t => t.Name == info.ClassName);
            this.SchemaUri = info.SchemaUri;

            if (!string.IsNullOrEmpty(info.CustomLoaderClassName))
            {
                this.Loader = Activator.CreateInstance(assembly.GetType(info.CustomLoaderClassName)) as ICustomDeserializer;
            }

            await Task.FromResult<object>(null);
            return;
        }
    }
}
