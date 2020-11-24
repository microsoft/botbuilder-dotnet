﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Invoke request body type for app-based link query.
    /// </summary>
    public partial class AppBasedLinkQuery
    {
        /// <summary>
        /// Initializes a new instance of the AppBasedLinkQuery class.
        /// </summary>
        public AppBasedLinkQuery()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AppBasedLinkQuery class.
        /// </summary>
        /// <param name="url">Url queried by user</param>
        public AppBasedLinkQuery(string url = default(string))
        {
            Url = url;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets url queried by user
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets state, which is the magic code for OAuth Flow.
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }
    }
}
