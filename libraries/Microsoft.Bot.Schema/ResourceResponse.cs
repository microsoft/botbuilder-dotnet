// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// A response containing a resource ID.
    /// </summary>
    public class ResourceResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceResponse"/> class.
        /// </summary>
        /// <param name="id">Id of the resource.</param>
        public ResourceResponse(string id = default)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets id of the resource.
        /// </summary>
        /// <value>The ID of the resource.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
