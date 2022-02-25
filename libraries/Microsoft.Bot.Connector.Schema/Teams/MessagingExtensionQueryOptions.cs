// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Messaging extension query options.
    /// </summary>
    public partial class MessagingExtensionQueryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionQueryOptions"/> class.
        /// </summary>
        public MessagingExtensionQueryOptions()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionQueryOptions"/> class.
        /// </summary>
        /// <param name="skip">Number of entities to skip.</param>
        /// <param name="count">Number of entities to fetch.</param>
        public MessagingExtensionQueryOptions(int? skip = default, int? count = default)
        {
            Skip = skip;
            Count = count;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets number of entities to skip.
        /// </summary>
        /// <value>The number of entities to skip.</value>
        [JsonProperty(PropertyName = "skip")]
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or sets number of entities to fetch.
        /// </summary>
        /// <value>The number of entities to fetch.</value>
        [JsonProperty(PropertyName = "count")]
        public int? Count { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
