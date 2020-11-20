// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Runtime
{
    /// <summary>
    /// Defines configuration-related constants.
    /// </summary>
    public static class ConfigurationConstants
    {
        /// <summary>
        /// The configuration key mapping to the value representing the application root path.
        /// </summary>
        public const string ApplicationRootKey = "applicationRoot";

        /// <summary>
        /// The configuration key mapping to the value representing the bot root path.
        /// </summary>
        public const string BotKey = "bot";

        /// <summary>
        /// The configuration key mapping to the value representing the default resource identifier
        /// of the dialog to be utilized as the root dialog of the bot.
        /// </summary>
        public const string DefaultRootDialogKey = "defaultRootDialog";
    }
}
