using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A Managed Identity implementation of the <see cref="ServiceClientCredentialsFactory"/> interface.
    /// </summary>
    public class ManagedIdentityServiceClientCredentialsFactory : ServiceClientCredentialsFactory
    {
        private readonly string _appId;
        private readonly string _tenantId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedIdentityServiceClientCredentialsFactory"/> class.
        /// </summary>
        /// <param name="appId">Client ID for the managed identity assigned to the bot.</param>
        /// <param name="tenantId">Tenant ID of the Azure AD tenant where the bot is created.</param>
        public ManagedIdentityServiceClientCredentialsFactory(string appId, string tenantId)
        {
            _appId = appId ?? throw new ArgumentNullException(nameof(appId));
            _tenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        }

        /// <inheritdoc />
        public override Task<bool> IsValidAppIdAsync(string appId, CancellationToken cancellationToken)
        {
            return Task.FromResult(appId == _appId);
        }

        /// <inheritdoc />
        public override Task<bool> IsAuthenticationDisabledAsync(CancellationToken cancellationToken)
        {
            // Auth is always enabled for MSI
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public override Task<ServiceClientCredentials> CreateCredentialsAsync(
            string appId, string audience, string loginEndpoint, bool validateAuthority, CancellationToken cancellationToken)
        {
            if (appId != _appId)
            {
                throw new InvalidOperationException("Invalid Managed ID.");
            }

            return Task.FromResult<ServiceClientCredentials>(new ManagedIdentityAppCredentials(_appId, _tenantId, audience));
        }

        /// <inheritdoc />
        public override Task<string> GetAuthTenantAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_tenantId);
        }
    }
}
