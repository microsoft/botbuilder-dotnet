// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.Runtime.Settings
{
    /// <summary>
    /// Defines options to be supplied to <see cref="CoreBot"/>.
    /// </summary>
    internal class CoreBotOptions
    {
        /// <summary>
        /// Gets or sets the default locale to be utilized by the bot. Defaults to 'en-US'.
        /// </summary>
        /// <value>
        /// The default locale to be utilized by the bot. Defaults to 'en-US'.
        /// </value>
        public string DefaultLocale { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier of the dialog to serve as the root dialog of the bot.
        /// </summary>
        /// <value>
        /// The resource identifier of the dialog to serve as the root dialog of the bot.
        /// </value>
        public string RootDialog { get; set; }
    }
}
