// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Cloud environments capture the environment specific Bot Framework Protocol auth code.
    /// </summary>
    public static class CloudEnvironment
    {
        /// <summary>
        /// Creates the appropriate cloud environment instance.
        /// </summary>
        /// <param name="channelService">The Channel Service.</param>
        /// <param name="toChannelFromBotLoginUrl">The to Channel from bot login url.</param>
        /// <param name="toChannelFromBotOAuthScope">The to Channel from bot oauth scope.</param>
        /// <param name="toBotFromChannelTokenIssuer">The to bot from Channel Token Issuer.</param>
        /// <param name="oAuthUrl">The oAuth url.</param>
        /// <param name="toBotFromChannelOpenIdMetadataUrl">The to bot from Channel Open Id Metadata url.</param>
        /// <param name="toBotFromEmulatorOpenIdMetadataUrl">The to bot from Emulator Open Id Metadata url.</param>
        /// <param name="callerId">The Microsoft app password.</param>
        /// <returns>A new cloud environment.</returns>
        public static ICloudEnvironment Create(
            string channelService,
            string toChannelFromBotLoginUrl,
            string toChannelFromBotOAuthScope,
            string toBotFromChannelTokenIssuer,
            string oAuthUrl,
            string toBotFromChannelOpenIdMetadataUrl,
            string toBotFromEmulatorOpenIdMetadataUrl,
            string callerId)
        {
            if (string.IsNullOrEmpty(channelService))
            {
                return new PublicCloudEnvironment();
            }
            else if (channelService == GovernmentAuthenticationConstants.ChannelService)
            {
                return new GovernmentCloudEnvironment();
            }
            else
            {
                return new ParameterizedCloudEnvironment(
                    channelService,
                    toChannelFromBotLoginUrl,
                    toChannelFromBotOAuthScope,
                    toBotFromChannelTokenIssuer,
                    oAuthUrl,
                    toBotFromChannelOpenIdMetadataUrl,
                    toBotFromEmulatorOpenIdMetadataUrl,
                    callerId);
            }
        }
    }
}
