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
        /// Initializes a new instance of the <see cref="ManagedIdentityAppCredentials"/> class.
        /// Managed Identity for AAD credentials auth and caching.
        /// </summary>
        /// <param name="appId">Client ID for the managed identity assigned to the bot.</param>
        /// <param name="audience">The id of the resource that is being accessed by the bot.</param>
        public ManagedIdentityAppCredentials(string appId, string audience)
            : base(null, null, null, audience)
        {
            MicrosoftAppId = appId ?? throw new ArgumentNullException(nameof(appId));
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
