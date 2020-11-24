// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// The status of a particular token
    /// </summary>
    public partial class TokenStatus
    {
        /// <summary>
        /// Initializes a new instance of the TokenStatus class.
        /// </summary>
        public TokenStatus()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TokenStatus class.
        /// </summary>
        /// <param name="channelId">The channelId of the token status pertains
        /// to</param>
        /// <param name="connectionName">The name of the connection the token
        /// status pertains to</param>
        /// <param name="hasToken">True if a token is stored for this
        /// ConnectionName</param>
        /// <param name="serviceProviderDisplayName">The display name of the
        /// service provider for which this Token belongs to</param>
        public TokenStatus(string channelId = default(string), string connectionName = default(string), bool? hasToken = default(bool?), string serviceProviderDisplayName = default(string))
        {
            ChannelId = channelId;
            ConnectionName = connectionName;
            HasToken = hasToken;
            ServiceProviderDisplayName = serviceProviderDisplayName;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the channelId of the token status pertains to
        /// </summary>
        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection the token status pertains
        /// to
        /// </summary>
        [JsonProperty(PropertyName = "connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets true if a token is stored for this ConnectionName
        /// </summary>
        [JsonProperty(PropertyName = "hasToken")]
        public bool? HasToken { get; set; }

        /// <summary>
        /// Gets or sets the display name of the service provider for which
        /// this Token belongs to
        /// </summary>
        [JsonProperty(PropertyName = "serviceProviderDisplayName")]
        public string ServiceProviderDisplayName { get; set; }

    }
}
