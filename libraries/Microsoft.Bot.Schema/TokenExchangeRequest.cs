// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Request payload to be sent to the Bot Framework Token Service for Single Sign On.
    /// If the URI is set to a custom scope, then Token Service will exchange the token in its cache for a token targeting the custom scope and return it in the response.
    /// If a Token is sent in the payload, then Token Service will exchange the token for a token targetting the scopes specified in the corresponding OAauth connection.
    /// </summary>
    public partial class TokenExchangeRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenExchangeRequest"/> class.
        /// </summary>
        public TokenExchangeRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenExchangeRequest"/> class.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <param name="token">Token.</param>
        public TokenExchangeRequest(string uri = default(string), string token = default(string))
        {
            Uri = uri;
            Token = token;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets a URI string.
        /// </summary>
        /// <value>The URI string.</value>
        [JsonProperty(PropertyName = "uri")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Uri { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets a token string.
        /// </summary>
        /// <value>The token.</value>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
