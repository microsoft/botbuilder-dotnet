// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Schema of the target resource for which the Bot Framework Token Service would exchange a cached token for a user. This class applies only to AAD V1 connections.
    /// </summary>
    public partial class AadResourceUrls
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
        public AadResourceUrls(IList<string> resourceUrls = default(IList<string>))
        {
            ResourceUrls = resourceUrls;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the URLs to the resource you want to connect to.
        /// </summary>
        /// <value>The URLs to the resources you want to connect to.</value>
        [JsonProperty(PropertyName = "resourceUrls")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<string> ResourceUrls { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
