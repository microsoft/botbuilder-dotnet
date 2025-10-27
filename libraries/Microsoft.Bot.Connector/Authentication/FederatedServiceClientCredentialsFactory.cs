// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A Federated Credentials implementation of the <see cref="ServiceClientCredentialsFactory"/> interface.
    /// </summary>
    public class FederatedServiceClientCredentialsFactory : ServiceClientCredentialsFactory
    {
        private readonly string _appId;
        private readonly string _clientId;
        private readonly string _tenantId;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IAuthorizationHeaderProvider _tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FederatedServiceClientCredentialsFactory"/> class.
        /// </summary>
        /// <param name="tokenProvider">The token provider.</param>
        /// <param name="appId">Microsoft application Id.</param>
        /// <param name="clientId">Managed Identity Client Id.</param>
        /// <param name="tenantId">The app tenant.</param>
        /// <param name="httpClient">A custom httpClient to use.</param>
        /// <param name="logger">A logger instance to use.</param>
        /// This enables authentication App Registration + Federated Credentials.
        public FederatedServiceClientCredentialsFactory(
            IAuthorizationHeaderProvider tokenProvider,
            string appId,
            string clientId,
            string tenantId = null,
            HttpClient httpClient = null,
            ILogger logger = null)
            : base()
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            _tokenProvider = tokenProvider;
            _appId = appId;
            _clientId = clientId;
            _tenantId = tenantId;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public override Task<bool> IsValidAppIdAsync(string appId, CancellationToken cancellationToken)
        {
            return Task.FromResult(appId == _appId);
        }

        /// <inheritdoc />
        public override Task<bool> IsAuthenticationDisabledAsync(CancellationToken cancellationToken)
        {
            // Auth is always enabled for Certificate.
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public override Task<ServiceClientCredentials> CreateCredentialsAsync(
            string appId, string audience, string loginEndpoint, bool validateAuthority, CancellationToken cancellationToken)
        {
            if (appId != _appId)
            {
                throw new InvalidOperationException("Invalid App ID.");
            }

            return Task.FromResult<ServiceClientCredentials>(new FederatedAppCredentials(
                _tokenProvider,
                _appId,
                _clientId,
                channelAuthTenant: _tenantId,
                oAuthScope: audience,
                customHttpClient: _httpClient,
                logger: _logger));
        }
    }
}
