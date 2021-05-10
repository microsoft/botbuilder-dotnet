// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.OAuth;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    internal static class UserTokenAccess
    {
        public static async Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, OAuthPromptSettings settings, string magicCode, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            if (userTokenClient != null)
            {
                return await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, settings.ConnectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
            }
            else if (turnContext.Adapter is IExtendedUserTokenProvider adapter)
            {
                return await adapter.GetUserTokenAsync(turnContext, settings.OAuthAppCredentials, settings.ConnectionName, magicCode, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }

        public static async Task<SignInResource> GetSignInResourceAsync(ITurnContext turnContext, OAuthPromptSettings settings, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            if (userTokenClient != null)
            {
                return await userTokenClient.GetSignInResourceAsync(settings.ConnectionName, turnContext.Activity, null, cancellationToken).ConfigureAwait(false);
            }
            else if (turnContext.Adapter is IExtendedUserTokenProvider adapter)
            {
                return await adapter.GetSignInResourceAsync(turnContext, settings.OAuthAppCredentials, settings.ConnectionName, turnContext.Activity.From.Id, null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }

        public static async Task SignOutUserAsync(ITurnContext turnContext, OAuthPromptSettings settings, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            if (userTokenClient != null)
            {
                await userTokenClient.SignOutUserAsync(turnContext.Activity.From.Id, settings.ConnectionName, turnContext.Activity.ChannelId, cancellationToken).ConfigureAwait(false);
            }
            else if (turnContext.Adapter is IExtendedUserTokenProvider adapter)
            {
                await adapter.SignOutUserAsync(turnContext, settings.OAuthAppCredentials, settings.ConnectionName, turnContext.Activity?.From?.Id, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }

        public static async Task<TokenResponse> ExchangeTokenAsync(ITurnContext turnContext, OAuthPromptSettings settings, TokenExchangeRequest tokenExchangeRequest, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            if (userTokenClient != null)
            {
                var userId = turnContext.Activity.From.Id;
                var channelId = turnContext.Activity.ChannelId;
                return await userTokenClient.ExchangeTokenAsync(userId, settings.ConnectionName, channelId, tokenExchangeRequest, cancellationToken).ConfigureAwait(false);
            }
            else if (turnContext.Adapter is IExtendedUserTokenProvider adapter)
            {
                return await adapter.ExchangeTokenAsync(turnContext, settings.ConnectionName, turnContext.Activity.From.Id, tokenExchangeRequest, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }

        public static async Task<IConnectorClient> CreateConnectorClientAsync(ITurnContext turnContext, string serviceUrl, ClaimsIdentity claimsIdentity, string audience, CancellationToken cancellationToken)
        {
            var connectorFactory = turnContext.TurnState.Get<ConnectorFactory>();
            if (connectorFactory != null)
            {
                return await connectorFactory.CreateAsync(serviceUrl, audience, cancellationToken).ConfigureAwait(false);
            }
            else if (turnContext.Adapter is IConnectorClientBuilder connectorClientProvider)
            {
                return await connectorClientProvider.CreateConnectorClientAsync(serviceUrl, claimsIdentity, audience, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("OAuth prompt is not supported by the current adapter");
            }
        }
    }
}
