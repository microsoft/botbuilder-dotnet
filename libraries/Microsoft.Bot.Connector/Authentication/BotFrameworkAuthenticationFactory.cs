// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A factory for <see cref="BotFrameworkAuthentication" /> which encapsulate the environment specific Bot Framework Protocol auth code.
    /// </summary>
    public static class BotFrameworkAuthenticationFactory
    {
        /// <summary>
        /// Creates the a <see cref="BotFrameworkAuthentication" /> instance for anonymous testing scenarios.
        /// </summary>
        /// <returns>A new <see cref="BotFrameworkAuthentication" /> instance.</returns>
        public static BotFrameworkAuthentication Create()
        {
            return Create(
                channelService: null,
                validateAuthority: false,
                toChannelFromBotLoginUrl: null,
                toChannelFromBotOAuthScope: null,
                toBotFromChannelTokenIssuer: null,
                oAuthUrl: null,
                toBotFromChannelOpenIdMetadataUrl: null,
                toBotFromEmulatorOpenIdMetadataUrl: null,
                callerId: null,
                credentialFactory: new PasswordServiceClientCredentialFactory(),
                authConfiguration: new AuthenticationConfiguration(),
                httpClientFactory: null,
                logger: null);
        }

        /// <summary>
        /// Creates the appropriate <see cref="BotFrameworkAuthentication" /> instance.
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
        /// <param name="credentialFactory">The <see cref="ServiceClientCredentialsFactory" /> to use to create credentials.</param>
        /// <param name="authConfiguration">The <see cref="AuthenticationConfiguration" /> to use.</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory" /> to use.</param>
        /// <param name="logger">The <see cref="ILogger" /> to use.</param>
        /// <returns>A new <see cref="BotFrameworkAuthentication" /> instance.</returns>
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
            IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(channelService))
            {
                // Public cloud - Allow parameters to be overridden by configuration
                return new ParameterizedBotFrameworkAuthentication(
                    validateAuthority,
                    string.IsNullOrWhiteSpace(toChannelFromBotLoginUrl) ? AuthenticationConstants.ToChannelFromBotLoginUrlTemplate : toChannelFromBotLoginUrl,
                    string.IsNullOrWhiteSpace(toChannelFromBotOAuthScope) ? AuthenticationConstants.ToChannelFromBotOAuthScope : toChannelFromBotOAuthScope,
                    string.IsNullOrWhiteSpace(toBotFromChannelTokenIssuer) ? AuthenticationConstants.ToBotFromChannelTokenIssuer : toBotFromChannelTokenIssuer,
                    string.IsNullOrWhiteSpace(oAuthUrl) ? AuthenticationConstants.OAuthUrl : oAuthUrl,
                    string.IsNullOrWhiteSpace(toBotFromChannelOpenIdMetadataUrl) ? AuthenticationConstants.ToBotFromChannelOpenIdMetadataUrl : toBotFromChannelOpenIdMetadataUrl,
                    string.IsNullOrWhiteSpace(toBotFromEmulatorOpenIdMetadataUrl) ? AuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl : toBotFromEmulatorOpenIdMetadataUrl,
                    string.IsNullOrWhiteSpace(callerId) ? CallerIdConstants.PublicAzureChannel : callerId,
                    credentialFactory,
                    authConfiguration,
                    httpClientFactory,
                    logger);
            }

            if (channelService == GovernmentAuthenticationConstants.ChannelService)
            {
                // US Government cloud
                return new ParameterizedBotFrameworkAuthentication(
                    validateAuthority,
                    GovernmentAuthenticationConstants.ToChannelFromBotLoginUrl,
                    GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope,
                    GovernmentAuthenticationConstants.ToBotFromChannelTokenIssuer,
                    GovernmentAuthenticationConstants.OAuthUrlGov,
                    GovernmentAuthenticationConstants.ToBotFromChannelOpenIdMetadataUrl,
                    GovernmentAuthenticationConstants.ToBotFromEmulatorOpenIdMetadataUrl,
                    CallerIdConstants.USGovChannel,
                    credentialFactory,
                    authConfiguration,
                    httpClientFactory,
                    logger);
            }

            throw new ArgumentException("The provided ChannelService value is not supported.");
        }
    }
}
