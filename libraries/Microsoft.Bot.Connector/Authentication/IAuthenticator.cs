// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    public interface IAuthenticator
    {
        public Task<AuthenticatorResult> GetTokenAsync(bool forceRefresh = false);
    }
}
