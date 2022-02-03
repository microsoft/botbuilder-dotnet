// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Represents a Facebook Bot User object.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class FacebookBotUser
    {
        /// <summary>
        /// Gets or sets the ID of the bot user.
        /// </summary>
        /// <value>The ID of the bot user.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
