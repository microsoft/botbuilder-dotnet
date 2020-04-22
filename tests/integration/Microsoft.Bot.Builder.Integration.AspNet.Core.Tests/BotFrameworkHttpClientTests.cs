using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        [Fact]
        public async void AddsRecipientAndSetsItBackToNull()
        {
            Func<HttpRequestMessage, Task<HttpResponseMessage>> verifyRequestAndCreateResponse = (HttpRequestMessage request) =>
            {
                var content = request.Content.ReadAsStringAsync().Result;
                var a = JsonConvert.DeserializeObject<Activity>(content);
                Assert.NotNull(a.Recipient);

                var response = new HttpResponseMessage(HttpStatusCode.OK);

                response.Content = new StringContent(new JObject { }.ToString());
                return Task.FromResult(response);
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => verifyRequestAndCreateResponse(request))
                .Verifiable();

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockCredentialProvider = new Mock<ICredentialProvider>();

            var client = new BotFrameworkHttpClient(httpClient, mockCredentialProvider.Object);
            var activity = new Activity
            {
                Conversation = new ConversationAccount()
            };

            await client.PostActivityAsync(string.Empty, string.Empty, new Uri("https://skillbot.com/api/messages"), new Uri("https://parentbot.com/api/messages"), "NewConversationId", activity);

            // Assert
            Assert.Null(activity.Recipient);
        }

        [Fact]
        public async void DoesNotOverwriteNonNullRecipientValues()
        {
            const string skillRecipientId = "skillBot";
            Func<HttpRequestMessage, Task<HttpResponseMessage>> verifyRequestAndCreateResponse = (HttpRequestMessage request) =>
            {
                var content = request.Content.ReadAsStringAsync().Result;
                var a = JsonConvert.DeserializeObject<Activity>(content);
                Assert.NotNull(a.Recipient);
                Assert.Equal(skillRecipientId, a.Recipient?.Id);
                var response = new HttpResponseMessage(HttpStatusCode.OK);

                response.Content = new StringContent(new JObject { }.ToString());
                return Task.FromResult(response);
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => verifyRequestAndCreateResponse(request))
                .Verifiable();

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockCredentialProvider = new Mock<ICredentialProvider>();

            var client = new BotFrameworkHttpClient(httpClient, mockCredentialProvider.Object);
            var activity = new Activity
            {
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount("skillBot")
            };

            await client.PostActivityAsync(string.Empty, string.Empty, new Uri("https://skillbot.com/api/messages"), new Uri("https://parentbot.com/api/messages"), "NewConversationId", activity);

            // Assert
            Assert.Equal("skillBot", activity?.Recipient?.Id);
        }
    }
}
