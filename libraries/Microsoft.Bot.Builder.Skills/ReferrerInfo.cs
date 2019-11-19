// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// A class containing information about the calling bot.
    /// </summary>
    public class ReferrerInfo
    {
        /// <summary>
        /// A unique key for this object that can be used to store it in dictionaries.
        /// </summary>
        public static readonly string Key = typeof(ReferrerInfo).FullName;

        /// <summary>
        /// Gets or sets the MicrosoftAppId of the calling bot.
        /// </summary>
        /// <value>
        /// The MicrosoftAppId of the calling bot.
        /// </value>
        public string FromBotId { get; set; }

        /// <summary>
        /// Gets or sets the MicrosoftAppId of the bot being called.
        /// </summary>
        /// <value>
        /// The MicrosoftAppId of the bot being called.
        /// </value>
        public string ToBotId { get; set; }

        /// <summary>
        /// Gets or sets the conversation ID for the calling bot.
        /// </summary>
        /// <value>
        /// The conversation ID for the calling bot.
        /// </value>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the ServiceUrl for the calling bot.
        /// </summary>
        /// <value>
        /// The ServiceUrl for the calling bot.
        /// </value>
        public Uri ServiceUrl { get; set; }
    }
}
