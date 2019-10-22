// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookBotUser
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
