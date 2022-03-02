// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// A request to receive a user token.
    /// </summary>
    public class TokenRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRequest"/> class.
        /// </summary>
        public TokenRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRequest"/> class.
        /// </summary>
        /// <param name="provider">The provider to request a user token
        /// from.</param>
        /// <param name="settings">A collection of settings for the specific
        /// provider for this request.</param>
        public TokenRequest(string provider = default, IDictionary<string, object> settings = default)
        {
            Provider = provider;
            Settings = settings;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the provider to request a user token from.
        /// </summary>
        /// <value>The provider to request a user token from.</value>
        [JsonPropertyName("provider")]
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets a collection of settings for the specific provider for
        /// this request.
        /// </summary>
        /// <value>The collection of settings for the specific provider for this request.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("settings")]
        public IDictionary<string, object> Settings { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
