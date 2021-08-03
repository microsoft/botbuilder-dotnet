// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Managed Service Identity auth implementation.
    /// </summary>
    public class ManagedIdentityAppCredentials : AppCredentials
    {
        /// <summary>
        /// The configuration property for Client ID of the Managed Identity.
        /// </summary>
        public const string ManagedIdKey = "ManagedId";

        /// <summary>
        /// The configuration property for Tenant ID of the Azure AD tenant.
        /// </summary>
        public const string TenantIdKey = "TenantId";

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedIdentityAppCredentials"/> class.
        /// Managed Identity for AAD credentials auth and caching.
        /// </summary>
        /// <param name="appId">Client ID for the managed identity assigned to the bot.</param>
        /// <param name="tenantId">Tenant ID of the Azure AD tenant where the bot is created.</param>
        /// <param name="audience">The id of the resource that is being accessed by the bot.</param>
        public ManagedIdentityAppCredentials(string appId, string tenantId, string audience)
            : base(null, null, null, audience)
        {
            MicrosoftAppId = appId ?? throw new ArgumentNullException(nameof(appId));
            AuthTenant = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        }

        /// <inheritdoc/>
        protected override Lazy<AdalAuthenticator> BuildAuthenticator()
        {
            // Should not be called, legacy
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Lazy<IAuthenticator> BuildIAuthenticator()
        {
            // TODO: constructor, test oauth scope for skills and channels, enable httpclient factory, logging, etc
            return new (
                () => new ManagedIdentityAuthenticator(MicrosoftAppId, OAuthScope),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
