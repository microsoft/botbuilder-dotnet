// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
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

            // TODO: Config Validations
            // 1. AppType can only be one of 3 values (if present) -- If not specified, default is MultiTenant.
            // 2. TenantId can be specified at anytime -- If specified, it will be added to allowed token issuers.
            // 3. For MSI -- AppId is required, and, Password must not be specified.
            // 4. For SingleTenant -- TenantId is required.

            _inner = appType switch
            {
                "UserAssignedMSI" => new ManagedIdentityServiceClientCredentialsFactory(appId),
                "SingleTenant" => new PasswordServiceClientCredentialFactory(appId, password, tenantId, httpClient, logger),
                _ => new PasswordServiceClientCredentialFactory(appId, password, httpClient, logger) // MultiTenant
            };
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
