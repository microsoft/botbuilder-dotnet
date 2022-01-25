// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    /// <summary>
    /// Class for defining implementation of the SlackAdapter Options.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class SlackAdapterOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the signatures of incoming requests should be verified.
        /// </summary>
        /// <value>
        /// A value indicating whether the signatures of incoming requests should be verified.
        /// </value>
        public bool VerifyIncomingRequests { get; set; } = true;
    }
}
