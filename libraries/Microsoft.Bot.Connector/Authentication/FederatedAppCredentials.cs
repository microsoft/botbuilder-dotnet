// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Federated Credentials auth implementation.
    /// </summary>
    public class FederatedAppCredentials : AppCredentials
    {
        private readonly string _clientId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FederatedAppCredentials"/> class.
        /// </summary>
        /// <param name="tokenProvider">The token provider.</param>
        /// <param name="appId">App ID for the Application.</param>
        /// <param name="clientId">Client ID for the managed identity assigned to the bot.</param>
        /// <param name="channelAuthTenant">Optional. The token tenant.</param>
        /// <param name="oAuthScope">Optional. The scope for the token.</param>
        /// <param name="customHttpClient">Optional <see cref="HttpClient"/> to be used when acquiring tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> to gather telemetry data while acquiring and managing credentials.</param>
        public FederatedAppCredentials(IAuthorizationHeaderProvider tokenProvider, string appId, string clientId, string channelAuthTenant = null, string oAuthScope = null, HttpClient customHttpClient = null, ILogger logger = null)
            : base(tokenProvider, channelAuthTenant, customHttpClient, logger, oAuthScope)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            MicrosoftAppId = appId;
            _clientId = clientId;
        }

        /// <inheritdoc/>
        protected override Lazy<IAuthenticator> BuildIAuthenticator()
        {
            return new Lazy<IAuthenticator>(
                () => new FederatedAuthenticator(TokenProvider, MicrosoftAppId, _clientId, OAuthEndpoint, OAuthScope, CustomHttpClient, Logger),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
