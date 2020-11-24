﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Messaging extension response
    /// </summary>
    public partial class MessagingExtensionResponse
    {
        /// <summary>
        /// Initializes a new instance of the MessagingExtensionResponse class.
        /// </summary>
        public MessagingExtensionResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MessagingExtensionResponse class.
        /// </summary>
        /// <param name="composeExtension">A <see cref="MessagingExtensionResult"/> that initializes the current object's ComposeExension property.</param>
        public MessagingExtensionResponse(MessagingExtensionResult composeExtension = default(MessagingExtensionResult))
        {
            ComposeExtension = composeExtension;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "composeExtension")]
        public MessagingExtensionResult ComposeExtension { get; set; }

        /// <summary>
        /// Gets or sets the CacheInfo for this MessagingExtensionResponse.
        /// module.
        /// </summary>
        [JsonProperty(PropertyName = "cacheInfo")]
        public CacheInfo CacheInfo { get; set; }
    }
}
