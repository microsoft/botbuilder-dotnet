// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// Response schema sent back from Bot Framework Token Service required to initiate a user single sign on.
    /// </summary>
    public class TokenExchangeResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenExchangeResource"/> class.
        /// </summary>
        /// <param name="id">ID.</param>
        /// <param name="uri">URI.</param>
        /// <param name="providerId">Identity provider ID.</param>
        public TokenExchangeResource(string id = default, string uri = default, string providerId = default)
        {
            Id = id;
            Uri = uri;
            ProviderId = providerId;
        }

        /// <summary>
        /// Gets or sets a unique identifier for this token exchange instance.
        /// </summary>
        /// <value>The ID for this token exchange instance.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the application ID / resource identifier with which to exchange a token on behalf of.
        /// </summary>
        /// <value>The URI.</value>
        [JsonProperty(PropertyName = "uri")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Uri { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets the identifier of the provider with which to attempt a token exchange
        /// A value of null or empty will default to Azure Active Directory.
        /// </summary>
        /// <value>The ID of the provider with which to attempt a tocken exchange.</value>
        [JsonProperty(PropertyName = "providerId")]
        public string ProviderId { get; set; }
    }
}
