using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Bot.Connector.Authentication;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class BotFrameworkHttpClientTests
    {
        [Fact]
        public void ConstructorValidations()
        {
            var mockHttpClient = new Mock<HttpClient>();
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            Assert.Throws<ArgumentNullException>(() =>
            {
                var client = new BotFrameworkHttpClient(null, mockCredentialProvider.Object);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                var client = new BotFrameworkHttpClient(mockHttpClient.Object, null);
            });
        }

        [Fact]
        public void ConstructorAddsHttpClientHeaders()
        {
            var httpClient = new HttpClient();
            var mockCredentialProvider = new Mock<ICredentialProvider>();

            Assert.False(httpClient.DefaultRequestHeaders.Any());
            var client = new BotFrameworkHttpClient(httpClient, mockCredentialProvider.Object);
            Assert.True(httpClient.DefaultRequestHeaders.Any());
        }
    }
}
