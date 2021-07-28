// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Credential factory that uses MSAL to acquire tokens.
    /// </summary>
    public class MsalServiceClientCredentialsFactory : ServiceClientCredentialsFactory
    {
        private readonly IConfidentialClientApplication _clientApplication;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalServiceClientCredentialsFactory"/> class.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> where to get the AppId from.</param>
        /// <param name="clientApplication"><see cref="IConfidentialClientApplication"/> used to acquire tokens.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> for credential acquisition telemetry.</param>
        public MsalServiceClientCredentialsFactory(IConfiguration configuration, IConfidentialClientApplication clientApplication, ILogger logger = null)
        {
            AppId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            _clientApplication = clientApplication;
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Gets the Microsoft App id.
        /// </summary>
        /// <value>
        /// The Microsoft App id.
        /// </value>
        public string AppId { get; }

        /// <inheritdoc/>
        public override Task<ServiceClientCredentials> CreateCredentialsAsync(string appId, string audience, string loginEndpoint, bool validateAuthority, CancellationToken cancellationToken)
        {
            // No Auth: let pass through.
            if (string.IsNullOrEmpty(AppId))
            {
                return Task.FromResult<ServiceClientCredentials>(MsalAppCredentials.Empty);
            }

            // If the member AppId is set, we expect the passed in appId to match.
            if (appId != AppId)
            {
                throw new InvalidOperationException($"Provided application Id '{appId}' does not match expected application Id '{AppId}'");
            }

            // Public cloud: default authority, optional scope when authenticating for skill communication.
            if (loginEndpoint.StartsWith(AuthenticationConstants.ToChannelFromBotLoginUrlTemplate, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ServiceClientCredentials>(
                    new MsalAppCredentials(_clientApplication, appId, authority: null, scope: audience, validateAuthority: validateAuthority, logger: _logger));
            }
            
            // Legacy gov: Set the authority (login url) to the legacy gov url, and allow for passed in scope for skill auth in
            // gov, or otherwise leave the default channel scope for gov.
            if (loginEndpoint.Equals(GovernmentAuthenticationConstants.ToChannelFromBotLoginUrl, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ServiceClientCredentials>(
                    new MsalAppCredentials(
                        _clientApplication, 
                        appId, 
                        authority: GovernmentAuthenticationConstants.ToChannelFromBotLoginUrl, 
                        scope: audience ?? GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope, 
                        validateAuthority: validateAuthority,
                        logger: _logger));
            }

            // Private cloud: use the passed in authority and scope since they were parametrized in a higher layer.
            return Task.FromResult<ServiceClientCredentials>(
                new MsalAppCredentials(_clientApplication, appId, authority: loginEndpoint, scope: audience, validateAuthority: validateAuthority, logger: _logger));
        }

        /// <inheritdoc/>
        public override Task<bool> IsAuthenticationDisabledAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(string.IsNullOrEmpty(AppId));
        }

        /// <inheritdoc/>
        public override Task<bool> IsValidAppIdAsync(string appId, CancellationToken cancellationToken)
        {
            return Task.FromResult(appId == AppId);
        }
    }
}
