// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    internal class ConfigurationResourceExplorer : ResourceExplorer
    {
        private readonly FolderResourceProvider _folderResourceProvider;

        public ConfigurationResourceExplorer(IConfiguration configuration)
        {
            var folder = configuration.GetSection(ConfigurationConstants.ApplicationRootKey).Value ?? AppContext.BaseDirectory;
            _folderResourceProvider = new FolderResourceProvider(this, folder, includeSubFolders: true, monitorChanges: true);
            AddResourceProvider(_folderResourceProvider);
            RegisterType<OnQnAMatch>(OnQnAMatch.Kind);
        }

        protected override void Dispose(bool disposing)
        {
            _folderResourceProvider.Dispose();
            base.Dispose(disposing);
        }
    }
}
