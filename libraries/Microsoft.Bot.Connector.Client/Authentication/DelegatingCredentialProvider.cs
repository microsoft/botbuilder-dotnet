// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Client.Authentication
{
    /// <summary>
    /// This is just an internal class to allow pre-existing implementation of the request validation to be used with a IServiceClientCredentialFactory.
    /// </summary>
    internal class DelegatingCredentialProvider : ICredentialProvider
    {
        private readonly ServiceClientCredentialsFactory _credentialFactory;

        public DelegatingCredentialProvider(ServiceClientCredentialsFactory credentialFactory)
        {
            _credentialFactory = credentialFactory ?? throw new ArgumentNullException(nameof(credentialFactory));
        }

        public Task<string> GetAppPasswordAsync(string appId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return _credentialFactory.IsAuthenticationDisabledAsync(CancellationToken.None);
        }

        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return _credentialFactory.IsValidAppIdAsync(appId, CancellationToken.None);
        }
    }
}
