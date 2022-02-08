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

#pragma warning disable 612, 618 //'member' is obsolete
        protected override Lazy<AdalAuthenticator> BuildAuthenticator()
        {
            return new Lazy<AdalAuthenticator>();
        }
#pragma warning restore 612, 618 //'member' is obsolete

    }
}
