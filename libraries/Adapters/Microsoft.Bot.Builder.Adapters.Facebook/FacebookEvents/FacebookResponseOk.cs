// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookResponseOk
    {
        [JsonProperty(PropertyName = "recipient_id")]
        public string RecipientId { get; set; }

        [JsonProperty(PropertyName = "message_id")]
        public string MessageId { get; set; }
    }
}
