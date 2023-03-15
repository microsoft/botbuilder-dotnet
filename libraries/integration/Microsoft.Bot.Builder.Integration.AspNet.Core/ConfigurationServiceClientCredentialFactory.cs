// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    internal enum MicrosoftAppType
    {
        /// <summary>
        /// MultiTenant app which uses botframework.com tenant to acquire tokens.
        /// </summary>
        MultiTenant,

        /// <summary>
        /// SingleTenant app which uses the bot's host tenant to acquire tokens.
        /// </summary>
        SingleTenant,

        /// <summary>
        /// App with a user assigned Managed Identity (MSI), which will be used as the AppId for token acquisition.
        /// </summary>
        UserAssignedMsi
    }

    /// <summary>
    /// Credential provider which uses <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> to lookup app credentials.
    /// </summary>
    public class ConfigurationServiceClientCredentialFactory : ServiceClientCredentialsFactory
    {
        private readonly ServiceClientCredentialsFactory _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationServiceClientCredentialFactory"/> class.
        /// </summary>
        /// <param name="configuration">An instance of <see cref="IConfiguration"/>.</param>
        /// <param name="httpClient">A httpClient to use.</param>
        /// <param name="logger">A logger to use.</param>
        public ConfigurationServiceClientCredentialFactory(IConfiguration configuration, HttpClient httpClient = null, ILogger logger = null)
        {
            var appType = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppTypeKey)?.Value;
            var appId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            var password = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value;
            var tenantId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppTenantIdKey)?.Value;

            var parsedAppType = Enum.TryParse(appType, ignoreCase: true, out MicrosoftAppType parsed)
                ? parsed
                : MicrosoftAppType.MultiTenant; // default

            switch (parsedAppType)
            {
                case MicrosoftAppType.UserAssignedMsi:
                    if (string.IsNullOrWhiteSpace(appId))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is required for MSI in configuration.");
                    }

                    if (string.IsNullOrWhiteSpace(tenantId))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppTenantIdKey} is required for MSI in configuration.");
                    }

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppPasswordKey} must not be set for MSI in configuration.");
                    }

                    _inner = new ManagedIdentityServiceClientCredentialsFactory(appId, httpClient, logger);
                    break;

                case MicrosoftAppType.SingleTenant:
                    if (string.IsNullOrWhiteSpace(appId))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is required for SingleTenant in configuration.");
                    }

                    if (string.IsNullOrWhiteSpace(tenantId))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppTenantIdKey} is required for SingleTenant in configuration.");
                    }

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppPasswordKey} is required for SingleTenant in configuration.");
                    }

                    _inner = new PasswordServiceClientCredentialFactory(appId, password, tenantId, httpClient, logger);
                    break;

                default: // MultiTenant
                    _inner = new PasswordServiceClientCredentialFactory(appId, password, tenantId: string.Empty,  httpClient, logger);
                    break;
            }
        }

        /// <inheritdoc />
        public override Task<bool> IsValidAppIdAsync(string appId, CancellationToken cancellationToken)
        {
            return _inner.IsValidAppIdAsync(appId, cancellationToken);
        }

        /// <inheritdoc />
        public override Task<bool> IsAuthenticationDisabledAsync(CancellationToken cancellationToken)
        {
            return _inner.IsAuthenticationDisabledAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override Task<ServiceClientCredentials> CreateCredentialsAsync(
            string appId, string audience, string loginEndpoint, bool validateAuthority, CancellationToken cancellationToken)
        {
            return _inner.CreateCredentialsAsync(
                appId, audience, loginEndpoint, validateAuthority, cancellationToken);
        }
    }
}
