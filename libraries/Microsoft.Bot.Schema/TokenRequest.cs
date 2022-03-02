// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A request to receive a user token.
    /// </summary>
    public class TokenRequest
    {
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
            Settings = settings ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the provider to request a user token from.
        /// </summary>
        /// <value>The provider to request a user token from.</value>
        [JsonProperty(PropertyName = "provider")]
        public string Provider { get; set; }

        /// <summary>
        /// Gets a collection of settings for the specific provider for
        /// this request.
        /// </summary>
        /// <value>The collection of settings for the specific provider for this request.</value>
        [JsonProperty(PropertyName = "settings")]
        public IDictionary<string, object> Settings { get; private set; } = new Dictionary<string, object>();
    }
}
