// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Events;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    /// <summary>
    /// SlackResponse class.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class SlackResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Ok status is true or false.
        /// </summary>
        /// <value>The ok status of the response.</value>
        public bool Ok { get; set; }

        /// <summary>
        /// Gets or sets the Channel property.
        /// </summary>
        /// <value>The channel.</value>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the Ts property.
        /// </summary>
        /// <value>The timestamp.</value>
        public string Ts { get; set; }

        /// <summary>
        /// Gets or sets the Message property.
        /// </summary>
        /// <value>The message.</value>
        public MessageEvent Message { get; set; }
    }
}
