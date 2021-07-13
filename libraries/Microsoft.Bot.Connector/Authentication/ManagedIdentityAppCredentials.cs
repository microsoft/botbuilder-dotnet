// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Managed Service Identity auth implementation.
    /// </summary>
    public class ManagedIdentityAppCredentials : AppCredentials
    {
        public ManagedIdentityAppCredentials() 
            : base(null, null, null)
        {
        }

        /// <summary>
        /// Gets or sets the ManagedIdentity tenant id.
        /// </summary>
        /// <value>
        /// The ManagedIdentity tenant id.
        /// </value>
        public string ManagedIdentityTenantId { get; set; }

        /// <summary>
        /// Gets or sets the ManagedIdentity client id.
        /// </summary>
        /// <value>
        /// The ManagedIdentity client id.
        /// </value>
        public string ManagedIdentityClientId { get; set; }

        /// <inheritdoc/>
        protected override Lazy<AdalAuthenticator> BuildAuthenticator()
        {
            // Should not be called, legacy
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Lazy<IAuthenticator> BuildIAuthenticator()
        {
            // TODOS: constructor, test oauth scope for skills and channels, enable httpclient factory, logging, etc
            return new Lazy<IAuthenticator>(
                () => new ManagedIdentityAuthenticator(ManagedIdentityTenantId, ManagedIdentityClientId, OAuthScope),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
