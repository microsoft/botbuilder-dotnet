using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class DelegatingHandlerTests
    {
        [TestMethod]
        public async Task EnsureDelegatingHandlerIsHonoredTest()
        {
            bool delegatingHandlerCalled = false;
            int numberOfRequestsProcessed = 0;
            TestDelegatingHandler testHandler = new TestDelegatingHandler((request) =>
            {
                JObject responseBody = null;
                if (request.RequestUri.AbsoluteUri.Equals("https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token", StringComparison.OrdinalIgnoreCase))
                {
                    responseBody = new JObject();
                    responseBody["token_type"] = "bearer";
                    responseBody["access_token"] = "fakeToken";
                }
                else if (request.RequestUri.AbsoluteUri.Equals("https://api.test.com/v3/conversations", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.IsTrue(request.Headers.Authorization.Parameter.Equals("fakeToken"));
                    responseBody = JObject.FromObject(new ConversationResourceResponse
                    {
                        ActivityId = "ActivityId",
                        Id = "TestConversationId",
                        ServiceUrl = "https://api.test.com"
                    });
                }
                else
                {
                    Assert.Fail("Unknown request");
                }

                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(responseBody));

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = stringContent
                };

                delegatingHandlerCalled = true;
                numberOfRequestsProcessed++;
                return Task.FromResult(response);
            });

            ConversationParameters conversationParameters = new ConversationParameters
            {
                Bot = new ChannelAccount
                {
                    Id = "Bot",
                    Name = "Bot"
                },
                Members = new List<ChannelAccount>
                {
                    new ChannelAccount
                    {
                        Id = "User",
                        Name = "User"
                    }
                },
            };

            MicrosoftAppCredentials.TrustServiceUrl("https://api.test.com");

            BotFrameworkAdapter botFrameworkAdapter = new BotFrameworkAdapter(
                credentialProvider: new SimpleCredentialProvider(),
                connectorClientRetryPolicy: null,
                delegatingHandler: testHandler,
                middleware: null);

            await botFrameworkAdapter.CreateConversation("testChannel", "https://api.test.com", null, conversationParameters, (turnContext) => { return Task.CompletedTask; });

            Assert.IsTrue(delegatingHandlerCalled, "DelegatingHandler was not called");
            Assert.AreEqual(2, numberOfRequestsProcessed);
        }

        private class TestDelegatingHandler : DelegatingHandler
        {
            /// <summary>
            /// The send function to be executed on request.
            /// </summary>
            private Func<HttpRequestMessage, Task<HttpResponseMessage>> sendFunc;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestDelegatingHandler"/> class.
            /// </summary>
            /// <param name="sendAsyncFunc">Function to be executed when request is made.</param>
            public TestDelegatingHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsyncFunc)
            {
                this.sendFunc = sendAsyncFunc;
            }

            /// <summary>
            /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
            /// </summary>
            /// <param name="request">The HTTP request message to send to the server.</param>
            /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
            /// <returns>
            /// Returns <see cref="Task" />. The task object representing the asynchronous operation.
            /// </returns>
            protected async override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return await this.sendFunc.Invoke(request).ConfigureAwait(false);
            }
        }
    }
}
