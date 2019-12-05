// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookBotUser
    {
        /// <summary>
        /// Gets or sets the Id of the bot user.
        /// </summary>
        /// <value>The Id of the bot user.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
