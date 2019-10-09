// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.Tests
{
    public class BotFrameworkHttpAdapterTests
    {
        [Fact]
        public async Task BasicMessageActivity()
        {
            // Arrange
            var httpRequest = new HttpRequestMessage();
            httpRequest.Content = CreateMessageActivityContent();

            var httpResponse = new HttpResponseMessage();

            // mock
            var botMock = new Mock<IBot>();
            botMock.Setup(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var adapter = new BotFrameworkHttpAdapter();
            await adapter.ProcessAsync(httpRequest, httpResponse, botMock.Object);

            // Assert
            botMock.Verify(m => m.OnTurnAsync(It.Is<TurnContext>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Once());
        }

        [Fact]
        public async Task InvokeActivity()
        {
            // Arrange
            var httpRequest = new HttpRequestMessage();
            httpRequest.Content = CreateInvokeActivityContent();

            var httpResponse = new HttpResponseMessage();

            var bot = new InvokeResponseBot();

            // Act
            var adapter = new BotFrameworkHttpAdapter();
            await adapter.ProcessAsync(httpRequest, httpResponse, bot);

            // Assert
            var s = await httpResponse.Content.ReadAsStringAsync();
            var json = JObject.Parse(s);
            Assert.Equal("im.feeling.really.attacked.right.now", json["quite.honestly"]);
        }

        [Fact]
        public async Task MessageActivityWithHttpClient()
        {
            // Arrange
            var httpRequest = new HttpRequestMessage();
            httpRequest.Content = CreateMessageActivityContent();

            var httpResponse = new HttpResponseMessage();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(CreateInternalHttpResponse()));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var bot = new MessageBot();

            // Act
            var adapter = new BotFrameworkHttpAdapter(null, null, httpClient, null);
            await adapter.ProcessAsync(httpRequest, httpResponse, bot);

            // Assert
            mockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task BadRequest()
        {
            // Arrange
            var httpRequest = new HttpRequestMessage();
            httpRequest.Content = new StringContent("this.is.not.json", Encoding.UTF8, "application/json");

            var httpResponse = new HttpResponseMessage();

            var botMock = new Mock<IBot>();
            botMock.Setup(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var adapter = new BotFrameworkHttpAdapter();
            await adapter.ProcessAsync(httpRequest, httpResponse, botMock.Object);

            // Assert
            botMock.Verify(m => m.OnTurnAsync(It.Is<TurnContext>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Never());
            Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        }

        private static HttpContent CreateMessageActivityContent()
        {
            return CreateContent(new Activity
            {
                Type = ActivityTypes.Message,
                Text = "hi",
                ServiceUrl = "http://localhost",
                ChannelId = "ChannelId",
                Conversation = new ConversationAccount { Id = "ConversationId" },
            });
        }

        private static HttpResponseMessage CreateInternalHttpResponse()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(new JObject { { "id", "SendActivityId" } }.ToString());
            return response;
        }

        private static HttpContent CreateInvokeActivityContent()
        {
            return CreateContent(new Activity { Type = ActivityTypes.Invoke, ServiceUrl = "http://localhost" });
        }

        private static HttpContent CreateContent(Activity activity)
        {
            string json = JsonConvert.SerializeObject(activity, MessageSerializerSettings.Create());
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private class InvokeResponseBot : IBot
        {
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                await turnContext.SendActivityAsync(CreateInvokeResponseActivity());
            }

            private static Activity CreateInvokeResponseActivity()
            {
                return new Activity
                {
                    Type = ActivityTypesEx.InvokeResponse,
                    Value = new InvokeResponse
                    {
                        Status = 200,
                        Body = new JObject { { "quite.honestly", "im.feeling.really.attacked.right.now" } },
                    },
                };
            }
        }

        private class MessageBot : IBot
        {
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("rage.rage.against.the.dying.of.the.light"));
            }
        }
    }
}
