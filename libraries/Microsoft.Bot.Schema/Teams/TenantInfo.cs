﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a tenant.
    /// </summary>
    public class TenantInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantInfo"/> class.
        /// </summary>
        /// <param name="id">Unique identifier representing a tenant.</param>
        public TenantInfo(string id = default)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets unique identifier representing a tenant.
        /// </summary>
        /// <value>The ID representing a tenant.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
