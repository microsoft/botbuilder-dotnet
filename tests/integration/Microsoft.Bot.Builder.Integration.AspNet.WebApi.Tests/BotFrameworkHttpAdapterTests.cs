// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Moq;
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

        private static HttpContent CreateMessageActivityContent()
        {
            return CreateContent(new Activity { Type = ActivityTypes.Message, Text = "hi", ServiceUrl = "http://localhost" });
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
    }
}
