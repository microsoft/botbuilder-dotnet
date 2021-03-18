using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Bot.Connector.Authentication;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class CloudBotFrameworkClientTests
    {
        [Fact]
        public void ConstructorValidations()
        {
            var client = new Mock<HttpClient>();
            var auth = new Mock<BotFrameworkAuthentication>();

            Assert.Throws<ArgumentNullException>(() => new CloudBotFrameworkClient(null, auth.Object));
            Assert.Throws<ArgumentNullException>(() => new CloudBotFrameworkClient(client.Object, null));
        }

        [Fact]
        public void ConstructorAddsHttpClientHeaders()
        {
            var client = new HttpClient();
            var auth = new Mock<BotFrameworkAuthentication>();

            Assert.False(client.DefaultRequestHeaders.Any());
            _ = new CloudBotFrameworkClient(client, auth.Object);
            Assert.True(client.DefaultRequestHeaders.Any());
        }
    }
}
