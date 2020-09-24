// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Represents a Facebook Bot User object.
    /// </summary>
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
