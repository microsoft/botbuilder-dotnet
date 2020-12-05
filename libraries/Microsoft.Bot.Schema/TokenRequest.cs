// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// A request to receive a user token.
    /// </summary>
    public partial class TokenRequest
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
        public TokenRequest(string provider = default(string), IDictionary<string, object> settings = default(IDictionary<string, object>))
        {
            Provider = provider;
            Settings = settings;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the provider to request a user token from.
        /// </summary>
        /// <value>The provider to request a user token from.</value>
        [JsonProperty(PropertyName = "provider")]
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets a collection of settings for the specific provider for
        /// this request.
        /// </summary>
        /// <value>The collection of settings for the specific provider for this request.</value>
        [JsonProperty(PropertyName = "settings")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IDictionary<string, object> Settings { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
