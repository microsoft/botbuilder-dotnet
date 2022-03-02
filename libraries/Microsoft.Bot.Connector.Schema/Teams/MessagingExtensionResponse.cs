// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Messaging extension response.
    /// </summary>
    public class MessagingExtensionResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionResponse"/> class.
        /// </summary>
        public MessagingExtensionResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionResponse"/> class.
        /// </summary>
        /// <param name="composeExtension">A <see cref="MessagingExtensionResult"/> that initializes the current object's ComposeExension property.</param>
        public MessagingExtensionResponse(MessagingExtensionResult composeExtension = default)
        {
            ComposeExtension = composeExtension;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the compose extension.
        /// </summary>
        /// <value>The compose extension.</value>
        [JsonPropertyName("composeExtension")]
        public MessagingExtensionResult ComposeExtension { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CacheInfo"/> for this <see cref="MessagingExtensionResponse"/>.
        /// module.
        /// </summary>
        /// <value>The <see cref="CacheInfo"/> for this <see cref="MessagingExtensionResponse"/>.</value>
        [JsonPropertyName("cacheInfo")]
        public CacheInfo CacheInfo { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
