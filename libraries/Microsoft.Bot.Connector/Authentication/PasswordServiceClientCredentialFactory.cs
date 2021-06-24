// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A simple implementation of the <see cref="ServiceClientCredentialsFactory"/> interface.
    /// </summary>
    public class PasswordServiceClientCredentialFactory : ServiceClientCredentialsFactory
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordServiceClientCredentialFactory"/> class.
        /// with empty credentials.
        /// </summary>
        public PasswordServiceClientCredentialFactory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordServiceClientCredentialFactory"/> class.
        /// with the provided credentials.
        /// </summary>
        /// <param name="appId">The app ID.</param>
        /// <param name="password">The app password.</param>
        /// <param name="httpClient">A custom httpClient to use.</param>
        /// <param name="logger">A logger instance to use.</param>
        public PasswordServiceClientCredentialFactory(string appId, string password, HttpClient httpClient, ILogger logger)
        {
            AppId = appId;
            Password = password;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets the app ID for this credential.
        /// </summary>
        /// <value>
        /// The app ID for this credential.
        /// </value>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the app password for this credential.
        /// </summary>
        /// <value>
        /// The app password for this credential.
        /// </value>
        public string Password { get; set; }

        /// <inheritdoc/>
        public override Task<bool> IsValidAppIdAsync(string appId, CancellationToken cancellationToken)
        {
            return Task.FromResult(appId == AppId);
        }

        /// <inheritdoc/>
        public override Task<bool> IsAuthenticationDisabledAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(string.IsNullOrEmpty(AppId));
        }

        /// <inheritdoc/>
        public override Task<ServiceClientCredentials> CreateCredentialsAsync(string appId, string oauthScope, string loginEndpoint, bool validateAuthority, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(AppId))
            {
                return Task.FromResult<ServiceClientCredentials>(MicrosoftAppCredentials.Empty);
            }

            if (appId != AppId)
            {
                throw new InvalidOperationException("Invalid appId");
            }

            if (loginEndpoint.StartsWith(AuthenticationConstants.ToChannelFromBotLoginUrlTemplate, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ServiceClientCredentials>(
                    AppId == null
                        ?
                    MicrosoftAppCredentials.Empty
                        :
                    new MicrosoftAppCredentials(appId, Password, _httpClient, _logger, oauthScope));
            }
            else if (loginEndpoint.Equals(GovernmentAuthenticationConstants.ToChannelFromBotLoginUrl, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ServiceClientCredentials>(
                    AppId == null
                        ?
                    MicrosoftGovernmentAppCredentials.Empty
                        :
                    new MicrosoftGovernmentAppCredentials(appId, Password, _httpClient, _logger, oauthScope));
            }
            else
            {
                return Task.FromResult<ServiceClientCredentials>(
                    AppId == null
                        ?
                    new PrivateCloudAppCredentials(null, null, null, null, null, loginEndpoint, validateAuthority)
                        :
                    new PrivateCloudAppCredentials(AppId, Password, _httpClient, _logger, oauthScope, loginEndpoint, validateAuthority));
            }
        }

        private class PrivateCloudAppCredentials : MicrosoftAppCredentials
        {
            private readonly string _oauthEndpoint;
            private readonly bool _validateAuthority;

            public PrivateCloudAppCredentials(string appId, string password, HttpClient customHttpClient, ILogger logger, string oAuthScope, string oauthEndpoint, bool validateAuthority)
            : base(appId, password, customHttpClient, logger, oAuthScope)
            {
                _oauthEndpoint = oauthEndpoint;
                _validateAuthority = validateAuthority;
            }

            public override string OAuthEndpoint
            {
                get { return _oauthEndpoint; }
            }

            public override bool ValidateAuthority
            {
                get { return _validateAuthority; }
            }
        }
    }
}
