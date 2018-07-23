// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Configuration;
using System.Linq;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.BotFramework
{
    /// <summary>
    /// Channel provider which uses <see cref="ConfigurationManager.AppSettings"/> to lookup the issuers and open id metadata urls.
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

        public ConfigurationChannelProvider()
        {
            this.Issuer = ConfigurationManager.AppSettings[ChannelIssuerKey];
            this.OpenIdMetadataUrl = ConfigurationManager.AppSettings[ChannelOpenIdMetadataUrlKey];
        }
    }
}
