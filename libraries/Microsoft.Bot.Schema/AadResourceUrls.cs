﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Schema of the target resource for which the Bot Framework Token Service would exchange a cached token for a user. This class applies only to AAD V1 connections.
    /// </summary>
    public partial class AadResourceUrls
    {
        /// <summary>
        /// Initializes a new instance of the AadResourceUrls class.
        /// </summary>
        public AadResourceUrls()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AadResourceUrls class.
        /// </summary>
        public AadResourceUrls(IList<string> resourceUrls = default(IList<string>))
        {
            ResourceUrls = resourceUrls;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "resourceUrls")]
        public IList<string> ResourceUrls { get; set; }

    }
}
