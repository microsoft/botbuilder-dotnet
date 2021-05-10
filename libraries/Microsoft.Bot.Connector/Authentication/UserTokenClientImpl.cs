// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    internal class UserTokenClientImpl : UserTokenClient
    {
        private readonly string _appId;
        private readonly OAuthClient _client;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private bool _disposed;

        public UserTokenClientImpl(
            string appId,
            ServiceClientCredentials credentials,
            string oauthEndpoint,
            HttpClient httpClient,
            ILogger logger)
        {
            _appId = appId;
            _httpClient = httpClient ?? new HttpClient();
            ConnectorClient.AddDefaultRequestHeaders(_httpClient);
            _client = new OAuthClient(credentials, _httpClient, true) { BaseUri = new Uri(oauthEndpoint) };
            _logger = logger ?? NullLogger.Instance;
        }

        public override async Task<TokenResponse> GetUserTokenAsync(string userId, string connectionName, string channelId, string magicCode, CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GetUserTokenAsync));
            }

            _ = userId ?? throw new ArgumentNullException(nameof(userId));
            _ = connectionName ?? throw new ArgumentNullException(nameof(connectionName));

            _logger.LogInformation($"GetTokenAsync ConnectionName: {connectionName}");
            return await _client.UserToken.GetTokenAsync(userId, connectionName, channelId, magicCode, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<SignInResource> GetSignInResourceAsync(string connectionName, Activity activity, string finalRedirect, CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GetSignInResourceAsync));
            }

            _ = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _ = activity ?? throw new ArgumentNullException(nameof(activity));

            _logger.LogInformation($"GetSignInResourceAsync ConnectionName: {connectionName}");
            var state = CreateTokenExchangeState(_appId, connectionName, activity);
            return await _client.GetSignInResourceAsync(state, null, null, finalRedirect, cancellationToken).ConfigureAwait(false);
        }

        public override async Task SignOutUserAsync(string userId, string connectionName, string channelId, CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SignOutUserAsync));
            }

            _ = userId ?? throw new ArgumentNullException(nameof(userId));
            _ = connectionName ?? throw new ArgumentNullException(nameof(connectionName));

            _logger.LogInformation($"SignOutAsync ConnectionName: {connectionName}");
            await _client.UserToken.SignOutAsync(userId, connectionName, channelId, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<TokenStatus[]> GetTokenStatusAsync(string userId, string channelId, string includeFilter, CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GetTokenStatusAsync));
            }

            _ = userId ?? throw new ArgumentNullException(nameof(userId));
            _ = channelId ?? throw new ArgumentNullException(nameof(channelId));

            _logger.LogInformation("GetTokenStatusAsync");
            var result = await _client.UserToken.GetTokenStatusAsync(userId, channelId, includeFilter, cancellationToken).ConfigureAwait(false);
            return result?.ToArray();
        }

        public override async Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(string userId, string connectionName, string[] resourceUrls, string channelId, CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(GetAadTokensAsync));
            }

            _ = userId ?? throw new ArgumentNullException(nameof(userId));
            _ = connectionName ?? throw new ArgumentNullException(nameof(connectionName));

            _logger.LogInformation($"GetAadTokensAsync ConnectionName: {connectionName}");
            return (Dictionary<string, TokenResponse>)await _client.UserToken.GetAadTokensAsync(userId, connectionName, new AadResourceUrls() { ResourceUrls = resourceUrls?.ToList() }, channelId, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<TokenResponse> ExchangeTokenAsync(string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExchangeTokenAsync));
            }

            _ = userId ?? throw new ArgumentNullException(nameof(userId));
            _ = connectionName ?? throw new ArgumentNullException(nameof(connectionName));

            _logger.LogInformation($"ExchangeAsyncAsync ConnectionName: {connectionName}");
            var result = await _client.ExchangeAsyncAsync(userId, connectionName, channelId, exchangeRequest, cancellationToken).ConfigureAwait(false);

            if (result is ErrorResponse errorResponse)
            {
                throw new InvalidOperationException($"Unable to exchange token: ({errorResponse?.Error?.Code}) {errorResponse?.Error?.Message}");
            }
            else if (result is TokenResponse tokenResponse)
            {
                return tokenResponse;
            }
            else
            {
                throw new InvalidOperationException($"ExchangeAsyncAsync returned improper result: {result.GetType()}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _client.Dispose();
            _httpClient?.Dispose();
            base.Dispose(disposing);
            _disposed = true;
        }
    }
}
