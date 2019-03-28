// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Connector.Tests
{
    using System;
    using System.Net.Http;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Xunit;

    public class ConnectorClientTest : BaseTest
    {
        [Fact]
        public void ConnectorClientWithCustomHttpClientAndMicrosoftCredentials()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient();

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connector = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            Assert.Equal(connector.HttpClient.BaseAddress, baseUri);
        }
    }
}
