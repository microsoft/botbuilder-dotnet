// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Messaging extension response.
    /// </summary>
    public partial class MessagingExtensionResponse
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
        [JsonProperty(PropertyName = "composeExtension")]
        public MessagingExtensionResult ComposeExtension { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CacheInfo"/> for this <see cref="MessagingExtensionResponse"/>.
        /// module.
        /// </summary>
        /// <value>The <see cref="CacheInfo"/> for this <see cref="MessagingExtensionResponse"/>.</value>
        [JsonProperty(PropertyName = "cacheInfo")]
        public CacheInfo CacheInfo { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
