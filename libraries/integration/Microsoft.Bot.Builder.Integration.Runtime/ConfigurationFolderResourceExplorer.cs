// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    /// <summary>
    /// An <see cref="ResourceExplorer"/> implementation, based on <see cref="FolderResourceExplorer"/> that can be created from <see cref="IConfiguration"/>.
    /// </summary>
    internal class ConfigurationFolderResourceExplorer : FolderResourceExplorer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFolderResourceExplorer"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> to use.</param>
        public ConfigurationFolderResourceExplorer(IConfiguration configuration)
            : base(configuration.GetSection(ConfigurationConstants.ApplicationRootKey).Value ?? AppContext.BaseDirectory)
        {
            RegisterType<OnQnAMatch>(OnQnAMatch.Kind);
        }
    }
}
