// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// This is just an internal class to allow pre-existing implementation of the request validation to be used with a IServiceClientCredentialFactory.
    /// </summary>
    internal class DelegatingCredentialProvider : ICredentialProvider
    {
        private IServiceClientCredentialsFactory _credentialFactory;

        public DelegatingCredentialProvider(IServiceClientCredentialsFactory credentialFactory)
        {
            _credentialFactory = credentialFactory;
        }

        public Task<string> GetAppPasswordAsync(string appId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return _credentialFactory.IsAuthenticationDisabledAsync();
        }

        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return _credentialFactory.IsValidAppIdAsync(appId);
        }
    }
}
