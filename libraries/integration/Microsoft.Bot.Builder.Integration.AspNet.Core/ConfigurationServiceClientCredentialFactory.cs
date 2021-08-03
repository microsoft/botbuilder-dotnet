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
    /// Credential provider which uses <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> to lookup appId and password.
    /// </summary>
    /// <remarks>
    /// This will populate the <see cref="PasswordServiceClientCredentialFactory.AppId"/> from an configuration entry with the key of <see cref="MicrosoftAppCredentials.MicrosoftAppIdKey"/>
    /// and the <see cref="PasswordServiceClientCredentialFactory.Password"/> from a configuration entry with the key of <see cref="MicrosoftAppCredentials.MicrosoftAppPasswordKey"/>.
    ///
    /// NOTE: if the keys are not present, a <c>null</c> value will be used.
    /// </remarks>
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
            string appId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            string password = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value;
            string managedId = configuration.GetSection(ManagedIdentityAppCredentials.ManagedIdKey)?.Value;
            string tenantId = configuration.GetSection(ManagedIdentityAppCredentials.TenantIdKey)?.Value;

            if (!string.IsNullOrWhiteSpace(managedId) && !string.IsNullOrWhiteSpace(tenantId))
            {
                // Both ManagedId and TenantId are present -- Use MSI auth
                _inner = new ManagedIdentityServiceClientCredentialsFactory(managedId, tenantId);
            }
            else
            {
                // Default to Password Auth
                _inner = new PasswordServiceClientCredentialFactory(appId, password, httpClient, logger);
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

        /// <inheritdoc />
        public override Task<string> GetAuthTenantAsync(CancellationToken cancellationToken)
        {
            return _inner.GetAuthTenantAsync(cancellationToken);
        }
    }
}
