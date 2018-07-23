// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.BotFramework
{
    /// <summary>
    /// Channel provider which uses <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> to lookup the issuer and open id metadata url.
    /// </summary>
    /// <remarks>
    /// This will populate the <see cref="SimpleChannelProvider.Issuer"/> from a configuration entry with the key of <see cref="ChannelIssuerKey"/>
    /// and the <see cref="SimpleChannelProvider.OpenIdMetadataUrl"/> from a configuration entry with the key of <see cref="ChannelOpenIdMetadataUrlKey"/>.
    ///
    /// NOTE: if the keys are not present, a <c>null</c> value will be used.
    /// </remarks>
    public sealed class ConfigurationChannelProvider : SimpleChannelProvider
    {
        /// <summary>
        /// The key for Issuers.
        /// </summary>
        public const string ChannelIssuerKey = "ChannelIssuer";

        /// <summary>
        /// The key for Open Id Metadata Urls.
        /// </summary>
        public const string ChannelOpenIdMetadataUrlKey = "ChannelOpenIdMetadataUrl";

        public ConfigurationChannelProvider(IConfiguration configuration)
        {
            this.Issuer = configuration.GetSection(ChannelIssuerKey)?.Value;
            this.OpenIdMetadataUrl = configuration.GetSection(ChannelOpenIdMetadataUrlKey)?.Value;
        }
    }
}
