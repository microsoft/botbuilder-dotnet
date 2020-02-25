// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookAttachment
    {
        /// <summary>
        /// Gets or sets the type of the attachment.
        /// </summary>
        /// <value>The type of attachment.
        /// May be "image", "audio", "video", "file", or "template".</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the payload of the attachment.
        /// </summary>
        /// <value>The payload of the attachment.</value>
        [JsonProperty(PropertyName = "payload")]
        public AttachmentPayload Payload { get; set; }
    }
}
