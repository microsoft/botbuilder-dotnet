// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Adaptive;

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    internal class ConfigurationAdaptiveDialogBot : AdaptiveDialogBot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialogBot"/> class using <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        /// <param name="storage">The <see cref="IStorage"/> implementation to use for this <see cref="Dialog"/>.</param>
        /// <param name="resourceExplorer">The Bot Builder <see cref="ResourceExplorer"/> to load the <see cref="Dialog"/> from.</param>
        /// <param name="botFrameworkClient">A <see cref="BotFrameworkClient"/> for making calls to Bot Builder Skills.</param>
        public ConfigurationAdaptiveDialogBot(
            IConfiguration configuration,
            ILogger<AdaptiveDialogBot> logger,
            ResourceExplorer resourceExplorer,
            IStorage storage = null,
            BotFrameworkClient botFrameworkClient = null)
            : base(resourceExplorer,
                 configuration.GetSection(ConfigurationConstants.RootDialogKey).Value,
                 configuration.GetSection(ConfigurationConstants.DefaultLocaleKey).Value,
                 logger,
                 storage,
                 botFrameworkClient)
        {
        }
    }
}
