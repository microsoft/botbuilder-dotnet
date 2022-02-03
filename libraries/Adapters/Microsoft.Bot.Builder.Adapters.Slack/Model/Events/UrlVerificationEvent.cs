// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Events
{
    /// <summary>
    /// Represents a Slack Url Verification event https://api.slack.com/events/url_verification.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class UrlVerificationEvent
    {
        public string Type { get; set; }

        public string Challenge { get; set; }

        public string Token { get; set; }
    }
}
