// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Plaintext/HTML representation of the content of the message.
    /// </summary>
    public class MessageActionsPayloadBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayloadBody"/> class.
        /// </summary>
        /// <param name="contentType">Type of the content. Possible values
        /// include: 'html', 'text'.</param>
        /// <param name="content">The content of the body.</param>
        public MessageActionsPayloadBody(string contentType = default, string content = default)
        {
            ContentType = contentType;
            Content = content;
        }

        /// <summary>
        /// Gets or sets type of the content. Possible values include: 'html',
        /// 'text'.
        /// </summary>
        /// <value>The content type.</value>
        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content of the body.
        /// </summary>
        /// <value>The content of the body.</value>
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
    }
}
