// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Describes a tenant
    /// </summary>
    public partial class TenantInfo
    {
        /// <summary>
        /// Initializes a new instance of the TenantInfo class.
        /// </summary>
        public TenantInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TenantInfo class.
        /// </summary>
        /// <param name="id">Unique identifier representing a tenant</param>
        public TenantInfo(string id = default(string))
        {
            Id = id;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets unique identifier representing a tenant
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

    }
}
