// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Microsoft.Bot.Connector.Client.Models;
using Microsoft.Bot.Connector.Client.Authentication;

namespace Microsoft.Bot.Connector.Client
{
    internal class UserTokenClientImpl : UserTokenClient
    {
        private readonly string _appId;

        private readonly UserTokenRestClient _userToken;
        private readonly BotSignInRestClient _botSignIn;

        public UserTokenClientImpl(BotFrameworkCredential credential, string appId, Uri endpoint)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            _appId = appId;

            var options = new ConnectorOptions();
            var diagnostics = new ClientDiagnostics(options);
            var pipeline = credential.IsAuthenticationDisabledAsync().GetAwaiter().GetResult()
                ? HttpPipelineBuilder.Build(options)
                : HttpPipelineBuilder.Build(options, new BearerTokenAuthenticationPolicy(credential.GetTokenCredential(), $"api://{appId}/.default"));

            _userToken = new UserTokenRestClient(diagnostics, pipeline, endpoint);
            _botSignIn = new BotSignInRestClient(diagnostics, pipeline, endpoint);
        }

        public override async Task<TokenResponse> GetTokenAsync(string userId, string connectionName, string channelId, string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                channelId = null; // pass null to avoid creation of query parameter
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = null; // pass null to avoid creation of query parameter
            }

            var response = await _userToken.GetTokenAsync(userId, connectionName, channelId, code, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<IReadOnlyDictionary<string, TokenResponse>> GetAadTokensAsync(string userId, string connectionName, string[] resourceUrls, string channelId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                channelId = null; // pass null to avoid creation of query parameter
            }

            var aadResourceUrls = new AadResourceUrls();
            foreach (var resourceUrl in resourceUrls)
            {
                aadResourceUrls.ResourceUrls.Add(resourceUrl);
            }

            var response = await _userToken.GetAadTokensAsync(userId, connectionName, aadResourceUrls, channelId, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task SignOutAsync(string userId, string connectionName, string channelId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                connectionName = null; // pass null to avoid creation of query parameter
            }

            if (string.IsNullOrWhiteSpace(channelId))
            {
                channelId = null; // pass null to avoid creation of query parameter
            }

            await _userToken.SignOutAsync(userId, connectionName, channelId, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<IReadOnlyList<TokenStatus>> GetTokenStatusAsync(string userId, string channelId, string include, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                channelId = null; // pass null to avoid creation of query parameter
            }

            if (string.IsNullOrWhiteSpace(include))
            {
                include = null; // pass null to avoid creation of query parameter
            }

            var response = await _userToken.GetTokenStatusAsync(userId, channelId, include, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<TokenResponse> ExchangeTokenAsync(string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken)
        {
            var response = await _userToken.ExchangeAsyncAsync(userId, connectionName, channelId, exchangeRequest, cancellationToken).ConfigureAwait(false);
            var result = response.Value;

            return result switch
            {
                TokenResponse t => t,
                ErrorResponse e => throw new InvalidOperationException($"Unable to exchange token: ({e.Error?.Code}) {e.Error?.Message}"),
                _ => throw new InvalidOperationException($"Token Exchange returned improper result: {result.GetType()}")
            };
        }

        public override async Task<SignInUrlResponse> GetSignInResourceAsync(string connectionName, Activity activity, string finalRedirect, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var state = CreateTokenExchangeState(_appId, connectionName, activity);

            var response = await _botSignIn.GetSignInResourceAsync(state, null, null, finalRedirect, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        /// <summary>
        /// Helper function to create the base64 encoded token exchange state used in GetSignInResourceAsync calls.
        /// </summary>
        /// <param name="appId">The appId to include in the token exchange state.</param>
        /// <param name="connectionName">The connectionName to include in the token exchange state.</param>
        /// <param name="activity">The <see cref="Activity"/> from which to derive the token exchange state.</param>
        /// <returns>base64 encoded token exchange state.</returns>
        private static string CreateTokenExchangeState(string appId, string connectionName, Activity activity)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var tokenExchangeState = new TokenExchangeState
            {
                ConnectionName = connectionName,
                Conversation = activity.GetConversationReference(),
                RelatesTo = activity.RelatesTo,
                MicrosoftAppId = appId,
            };

            var json = JsonSerializer.Serialize(tokenExchangeState);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }
    }
}
