// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// A response containing a resource ID.
    /// </summary>
    public partial class ResourceResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceResponse"/> class.
        /// </summary>
        public ResourceResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceResponse"/> class.
        /// </summary>
        /// <param name="id">Id of the resource.</param>
        public ResourceResponse(string id = default)
        {
            Id = id;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets id of the resource.
        /// </summary>
        /// <value>The ID of the resource.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
