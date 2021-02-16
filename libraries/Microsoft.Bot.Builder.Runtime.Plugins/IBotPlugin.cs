// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Runtime.Plugins
{
    /// <summary>
    /// Represents a plugin to be utilized within the bot runtime that can provide access to
    /// additional components that implement well-defined interfaces in the Bot Framework SDK.
    /// </summary>
    public interface IBotPlugin
    {
        /// <summary>
        /// Load the contents of the plugin into the bot runtime.
        /// </summary>
        /// <param name="context">
        /// Load context that provides access to application configuration and service collection.
        /// </param>
        void Load(IBotPluginLoadContext context);
    }
}
