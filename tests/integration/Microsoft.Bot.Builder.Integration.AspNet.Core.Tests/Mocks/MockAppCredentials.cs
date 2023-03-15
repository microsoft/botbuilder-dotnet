// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests.Mocks
{
    internal class MockAppCredentials : AppCredentials
    {
        public MockAppCredentials(string channelAuthTenant = null, HttpClient customHttpClient = null, ILogger logger = null)
            : base(channelAuthTenant, customHttpClient, logger)
        {
        }
        
        [Obsolete("This method is deprecated. Use BuildIAuthenticator instead.", false)]
        protected override Lazy<AdalAuthenticator> BuildAuthenticator()
        {
            return new Lazy<AdalAuthenticator>();
        }
    }
}
