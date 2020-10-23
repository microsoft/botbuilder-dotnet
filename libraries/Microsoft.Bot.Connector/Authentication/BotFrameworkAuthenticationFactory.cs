// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Cloud environments capture the environment specific Bot Framework Protocol auth code.
    /// </summary>
    public static class BotFrameworkAuthenticationFactory
    {
        /// <summary>
        /// Creates the appropriate cloud environment instance.
        /// </summary>
        /// <param name="channelService">The Channel Service.</param>
        /// <param name="validateAuthority">The validate authority value to use.</param>
        /// <param name="toChannelFromBotLoginUrl">The to Channel from bot login url.</param>
        /// <param name="toChannelFromBotOAuthScope">The to Channel from bot oauth scope.</param>
        /// <param name="toBotFromChannelTokenIssuer">The to bot from Channel Token Issuer.</param>
        /// <param name="oAuthUrl">The oAuth url.</param>
        /// <param name="toBotFromChannelOpenIdMetadataUrl">The to bot from Channel Open Id Metadata url.</param>
        /// <param name="toBotFromEmulatorOpenIdMetadataUrl">The to bot from Emulator Open Id Metadata url.</param>
        /// <param name="callerId">The Microsoft app password.</param>
        /// <param name="credentialFactory">The IServiceClientCredentialsFactory to use to create credentials.</param>
        /// <param name="authConfiguration">The AuthenticationConfiguration to use.</param>
        /// <param name="httpClient">The HttpClient to use.</param>
        /// <param name="logger">The ILogger instance to use.</param>
        /// <returns>A new cloud environment.</returns>
        public static BotFrameworkAuthentication Create(
            string channelService,
            bool validateAuthority,
            string toChannelFromBotLoginUrl,
            string toChannelFromBotOAuthScope,
            string toBotFromChannelTokenIssuer,
            string oAuthUrl,
            string toBotFromChannelOpenIdMetadataUrl,
            string toBotFromEmulatorOpenIdMetadataUrl,
            string callerId,
            ServiceClientCredentialsFactory credentialFactory,
            AuthenticationConfiguration authConfiguration,
            HttpClient httpClient,
            ILogger logger)
        {
            if (
                !string.IsNullOrEmpty(toChannelFromBotLoginUrl) || 
                !string.IsNullOrEmpty(toChannelFromBotOAuthScope) || 
                !string.IsNullOrEmpty(toBotFromChannelTokenIssuer) || 
                !string.IsNullOrEmpty(oAuthUrl) ||
                !string.IsNullOrEmpty(toBotFromChannelOpenIdMetadataUrl) ||
                !string.IsNullOrEmpty(toBotFromEmulatorOpenIdMetadataUrl) ||
                !string.IsNullOrEmpty(callerId))
            {
                // if we have any of the 'parameterized' properties defined we'll assume this is the parameterized code

                return new ParameterizedBotFrameworkAuthentication(
                    validateAuthority,
                    toChannelFromBotLoginUrl,
                    toChannelFromBotOAuthScope,
                    toBotFromChannelTokenIssuer,
                    oAuthUrl,
                    toBotFromChannelOpenIdMetadataUrl,
                    toBotFromEmulatorOpenIdMetadataUrl,
                    callerId,
                    credentialFactory,
                    authConfiguration,
                    httpClient,
                    logger);
            }
            else
            {
                // else apply the built in default behavior, which is either the public cloud or the gov cloud depending on whether we have a channelService value present 

                if (string.IsNullOrEmpty(channelService))
                {
                    return new PublicCloudBotFrameworkAuthentication(credentialFactory, authConfiguration, httpClient, logger);
                }
                else if (channelService == GovernmentAuthenticationConstants.ChannelService)
                {
                    return new GovernmentCloudBotFrameworkAuthentication(credentialFactory, authConfiguration, httpClient, logger);
                }
                else
                {
                    // The ChannelService value is used an indicator of which built in set of constants to use. If it is not recognized, a full configuration is expected.

                    throw new ArgumentException("The provided ChannelService value is not supported.");
                }
            }
        }
    }
}
