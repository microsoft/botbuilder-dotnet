// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.Runtime
{
    /// <summary>
    /// Defines configuration-related constants.
    /// </summary>
    internal static class ConfigurationConstants
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
        /// of the dialog to be used as the root dialog of the bot.
        /// </summary>
        public const string RootDialogKey = "defaultRootDialog";

        /// <summary>
        /// The configuration key mapping to the value representing the default resource identifier
        /// of the LanguageGenerator to be used by the bot.
        /// </summary>
        public const string LanguageGeneratorKey = "defaultLg";

        /// <summary>
        /// Default configuration location for runtime settings.
        /// </summary>
        public const string RuntimeSettingsKey = "runtimeSettings";

        /// <summary>
        /// The configuration key mapping to the value representing the default locale.
        /// </summary>
        public const string DefaultLocaleKey = "defaultLocale";
    }
}
