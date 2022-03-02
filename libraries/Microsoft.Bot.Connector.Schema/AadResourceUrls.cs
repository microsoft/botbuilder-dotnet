// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Schema of the target resource for which the Bot Framework Token Service would exchange a cached token for a user. This class applies only to AAD V1 connections.
    /// </summary>
    public class AadResourceUrls
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AadResourceUrls"/> class.
        /// </summary>
        public AadResourceUrls()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AadResourceUrls"/> class.
        /// </summary>
        /// <param name="resourceUrls">The URLs to the resource you want to connect to.</param>
        public AadResourceUrls(IList<string> resourceUrls = default)
        {
            ResourceUrls = resourceUrls;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the URLs to the resource you want to connect to.
        /// </summary>
        /// <value>The URLs to the resources you want to connect to.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("resourceUrls")]
        public IList<string> ResourceUrls { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
