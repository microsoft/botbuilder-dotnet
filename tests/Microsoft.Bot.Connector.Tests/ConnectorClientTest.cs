// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
{
    public class ConnectorClientTest : BaseTest
    {
        [Fact]
        public void ConnectorClient_CustomHttpClient_AndMicrosoftCredentials()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient();

            // Set a special base address so then we can make sure the connector client is honoring this http client
            customHttpClient.BaseAddress = baseUri;
            var connector = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient);

            Assert.Equal(connector.HttpClient.BaseAddress, baseUri);
        }

        [Fact]
        public async Task ConnectorClient_CustomHttpClientAndCredConstructor_HttpClientDisposed()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient();

            using (var connector = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient))
            { 
                // Use the connector
            }

            await Assert.ThrowsAsync<ObjectDisposedException>(() => customHttpClient.GetAsync("http://bing.com"));
        }

        [Fact]
        public async Task ConnectorClient_CustomHttpClientAndDisposeFalse_HttpClientNotDisposed()
        {
            var baseUri = new Uri("https://test.coffee");
            var customHttpClient = new HttpClient();

            using (var connector = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), customHttpClient, disposeHttpClient: customHttpClient == null))
            {
                // Use the connector
            }

            // If the HttpClient were disposed, this would throw ObjectDisposedException
            await customHttpClient.GetAsync("http://bing.com");
        }

        [Fact]
        public void ConnectorClient_CustomHttpClientNull_Works()
        {
            var baseUri = new Uri("https://test.coffee");

            using (var connector = new ConnectorClient(new Uri("http://localhost/"), new MicrosoftAppCredentials(string.Empty, string.Empty), null, disposeHttpClient: true))
            {
                // Use the connector
            }
        }
    }
}
