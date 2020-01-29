// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookResponseOk
    {
        /// <summary>
        /// Gets or sets the recipient Id.
        /// </summary>
        /// <value>The Id of the recipient.</value>
        [JsonProperty(PropertyName = "recipient_id")]
        public string RecipientId { get; set; }

        /// <summary>
        /// Gets or sets the message Id.
        /// </summary>
        /// <value>The message id.</value>
        [JsonProperty(PropertyName = "message_id")]
        public string MessageId { get; set; }
    }
}
