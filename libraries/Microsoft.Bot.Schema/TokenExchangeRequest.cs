﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

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
        /// Initializes a new instance of the TokenExchangeRequest class.
        /// </summary>
        public TokenExchangeRequest(string uri = default(string), string token = default(string))
        {
            Uri = uri;
            Token = token;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets a URI string.
        /// </summary>
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets a token string.
        /// </summary>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

    }
}
