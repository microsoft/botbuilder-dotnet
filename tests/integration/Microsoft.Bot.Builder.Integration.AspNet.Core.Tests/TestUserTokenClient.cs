// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    internal class TestUserTokenClient : UserTokenClient
    {
        private string v;

        public TestUserTokenClient(string v)
        {
            this.v = v;
        }

        public override Task<TokenResponse> ExchangeTokenAsync(string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(string userId, string connectionName, string[] resourceUrls, string channelId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<SignInResource> GetSignInResourceAsync(string connectionName, Activity activity, string finalRedirect, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<TokenStatus[]> GetTokenStatusAsync(string userId, string channelId, string includeFilter, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<TokenResponse> GetUserTokenAsync(string userId, string connectionName, string channelId, string magicCode, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task SignOutUserAsync(string userId, string connectionName, string channelId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
