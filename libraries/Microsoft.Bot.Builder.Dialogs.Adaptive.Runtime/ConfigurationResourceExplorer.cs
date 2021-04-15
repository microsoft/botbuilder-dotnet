// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime
{
    internal class ConfigurationResourceExplorer : ResourceExplorer
    {
        private readonly FolderResourceProvider _folderResourceProvider;

        public ConfigurationResourceExplorer(
            IConfiguration configuration,
            IEnumerable<DeclarativeType> declarativeTypes,
            IEnumerable<JsonConverterFactory> converterFactories)
            : base(new ResourceExplorerOptions() { ConverterFactories = converterFactories, TypeRegistrations = declarativeTypes })
        {
            var folder = configuration.GetSection(ConfigurationConstants.ApplicationRootKey).Value ?? AppContext.BaseDirectory;
            _folderResourceProvider = new FolderResourceProvider(this, folder, includeSubFolders: true, monitorChanges: true);
            AddResourceProvider(_folderResourceProvider);
        }

        protected override void Dispose(bool disposing)
        {
            _folderResourceProvider.Dispose();
            base.Dispose(disposing);
        }
    }
}
