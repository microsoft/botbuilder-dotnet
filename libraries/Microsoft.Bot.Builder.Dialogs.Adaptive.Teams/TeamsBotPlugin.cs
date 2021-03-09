// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams
{
    /// <summary>
    /// Implementation of <see cref="IBotPlugin"/> for registering Teams adaptive components.
    /// </summary>
    public class TeamsBotPlugin : IBotPlugin
    {
        /// <inheritdoc/>
        public void Load(IBotPluginLoadContext context)
        {
            ComponentRegistration.Add(new TeamsComponentRegistration());
        }
    }
}
