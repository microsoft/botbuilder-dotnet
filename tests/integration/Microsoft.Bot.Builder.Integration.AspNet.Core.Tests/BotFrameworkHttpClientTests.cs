// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
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
                _ = new BotFrameworkHttpClient(null, mockCredentialProvider.Object);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new BotFrameworkHttpClient(mockHttpClient.Object, null);
            });
        }

        [Fact]
        public void ConstructorAddsHttpClientHeaders()
        {
            var httpClient = new HttpClient();
            var mockCredentialProvider = new Mock<ICredentialProvider>();

            Assert.False(httpClient.DefaultRequestHeaders.Any());
            _ = new BotFrameworkHttpClient(httpClient, mockCredentialProvider.Object);
            Assert.True(httpClient.DefaultRequestHeaders.Any());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("{ \"id\": \"someId\", \"someProp\": \"theProp\" }")]
        public async Task PostActivityUsingInvokeResponse(string expectedResponseBodyJson)
        {
            var httpClient = CreateHttpClientWithMockHandler((request, cancellationToken) =>
            {
                Assert.Equal("NewConversationId", request.Headers.GetValues(ConversationConstants.ConversationIdHttpHeaderName).FirstOrDefault());
                var sentActivity = JsonConvert.DeserializeObject<Activity>(request.Content.ReadAsStringAsync().Result);
                Assert.Equal(RoleTypes.Skill, sentActivity.Recipient.Role);

                // Create mock response.
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = expectedResponseBodyJson == null ? null : new StringContent(expectedResponseBodyJson)
                };
                return Task.FromResult(response);
            });

            var sut = new BotFrameworkHttpClient(httpClient, new Mock<ICredentialProvider>().Object);
            var activity = new Activity { Conversation = new ConversationAccount() };
            var result = await sut.PostActivityAsync(string.Empty, string.Empty, new Uri("https://skillbot.com/api/messages"), new Uri("https://parentbot.com/api/messages"), "NewConversationId", activity);

            // Assert
            Assert.IsType<InvokeResponse<object>>(result);
            Assert.Equal(200, result.Status);
            if (expectedResponseBodyJson == null)
            {
                Assert.Null(result.Body);
            }
            else
            {
                var typedContent = JsonConvert.DeserializeObject<TestContentBody>(JsonConvert.SerializeObject(result.Body));
                Assert.Equal("someId", typedContent.Id);
                Assert.Equal("theProp", typedContent.SomeProp);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("42")]
        public async Task PostActivityUsingInvokeResponseOfT(string testRecipientId)
        {
            // The test activity being sent.
            var testActivity = new Activity
            {
                Conversation = new ConversationAccount
                {
                    Id = "TheActivityConversationId",
                    Name = "TheActivityConversationName",
                    ConversationType = "TheActivityConversationType",
                    AadObjectId = "TheActivityAadObjectId",
                    IsGroup = true,
                    Properties = new JObject(),
                    Role = "TheActivityRole",
                    TenantId = "TheActivityTenantId",
                },
                ServiceUrl = "https://theactivityServiceUrl",
                Recipient = testRecipientId == null ? null : new ChannelAccount(testRecipientId),
                RelatesTo = null,
            };

            var httpClient = CreateHttpClientWithMockHandler((request, cancellationToken) =>
            {
                // Assert the request properties
                Assert.Equal(new Uri("https://skillbot.com/api/messages"), request.RequestUri);
                Assert.Equal("NewConversationId", request.Headers.GetValues(ConversationConstants.ConversationIdHttpHeaderName).FirstOrDefault());

                // Assert expected values are in the activity being sent.
                var sentActivity = JsonConvert.DeserializeObject<Activity>(request.Content.ReadAsStringAsync().Result);
                Assert.Equal(RoleTypes.Skill, sentActivity.Recipient.Role);

                Assert.Equal("https://parentbot.com/api/messages", sentActivity.ServiceUrl);
                Assert.Equal("NewConversationId", sentActivity.Conversation.Id);
                if (testRecipientId == null)
                {
                    // Ensure a default recipient is set if we don't pass one.
                    Assert.NotNull(sentActivity.Recipient);
                }
                else
                {
                    // Ensure the recipient we want is set if it is passed.
                    Assert.Equal("42", sentActivity.Recipient.Id);
                }

                Assert.NotNull(sentActivity.RelatesTo);
                Assert.Equal(testActivity.Conversation.Id, sentActivity.RelatesTo.Conversation.Id);
                Assert.Equal("TheActivityConversationName", sentActivity.RelatesTo.Conversation.Name);
                Assert.Equal("TheActivityConversationType", sentActivity.RelatesTo.Conversation.ConversationType);
                Assert.Equal("TheActivityAadObjectId", sentActivity.RelatesTo.Conversation.AadObjectId);
                Assert.Equal(true, sentActivity.RelatesTo.Conversation.IsGroup);
                Assert.NotNull(sentActivity.RelatesTo.Conversation.Properties);
                Assert.Equal("TheActivityRole", sentActivity.RelatesTo.Conversation.Role);
                Assert.Equal("TheActivityTenantId", sentActivity.RelatesTo.Conversation.TenantId);
                Assert.Equal("https://theactivityServiceUrl", sentActivity.RelatesTo.ServiceUrl);

                // Create mock response.
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JObject.FromObject(new TestContentBody("someId", "theProp")).ToString())
                };
                return Task.FromResult(response);
            });

            var client = new BotFrameworkHttpClient(httpClient, new Mock<ICredentialProvider>().Object);
            var result = await client.PostActivityAsync<TestContentBody>(string.Empty, string.Empty, new Uri("https://skillbot.com/api/messages"), new Uri("https://parentbot.com/api/messages"), "NewConversationId", testActivity);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.IsType<InvokeResponse<TestContentBody>>(result);
            Assert.NotNull(result.Body);

            var typedContent = JsonConvert.DeserializeObject<TestContentBody>(JsonConvert.SerializeObject(result.Body));
            Assert.Equal("someId", typedContent.Id);
            Assert.Equal("theProp", typedContent.SomeProp);
        }

        [Fact]
        public async Task PostActivityUsingInvokeResponseToSelf()
        {      
            var activity = new Activity { Conversation = new ConversationAccount(id: Guid.NewGuid().ToString()) };
            var httpClient = CreateHttpClientWithMockHandler((request, cancellationToken) =>
            {
                Assert.Equal(activity.Conversation.Id, request.Headers.GetValues(ConversationConstants.ConversationIdHttpHeaderName).FirstOrDefault());
                var sentActivity = JsonConvert.DeserializeObject<Activity>(request.Content.ReadAsStringAsync().Result);

                // Assert the activity we are sending is what we passed in.
                Assert.Equal(activity.Conversation.Id, sentActivity.Conversation.Id);

                // Create mock response.
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = null
                };
                return Task.FromResult(response);
            });
            
            var client = new BotFrameworkHttpClient(httpClient, new Mock<ICredentialProvider>().Object);
            var result = await client.PostActivityAsync(string.Empty, new Uri("https://skillbot.com/api/messages"), activity);

            Assert.IsType<InvokeResponse<object>>(result);
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.Null(result.Body);
        }

        [Fact]
        public async Task PostActivityUsingInvokeResponseOfTToSelf()
        {      
            var activity = new Activity { Conversation = new ConversationAccount(id: Guid.NewGuid().ToString()) };
            var httpClient = CreateHttpClientWithMockHandler((request, cancellationToken) =>
            {
                Assert.Equal(activity.Conversation.Id, request.Headers.GetValues(ConversationConstants.ConversationIdHttpHeaderName).FirstOrDefault());
                var sentActivity = JsonConvert.DeserializeObject<Activity>(request.Content.ReadAsStringAsync().Result);

                // Assert the activity we are sending is what we passed in.
                Assert.Equal(activity.Conversation.Id, sentActivity.Conversation.Id);

                // Create mock response.
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JObject.FromObject(new TestContentBody("someId", "theProp")).ToString())
                };
                return Task.FromResult(response);
            });
            
            var client = new BotFrameworkHttpClient(httpClient, new Mock<ICredentialProvider>().Object);
            var result = await client.PostActivityAsync<TestContentBody>(string.Empty, new Uri("https://skillbot.com/api/messages"), activity);

            Assert.IsType<InvokeResponse<TestContentBody>>(result);
            Assert.Equal((int)HttpStatusCode.OK, result.Status);
            Assert.NotNull(result.Body);
        }

        /// <summary>
        /// Helper to create an HttpClient with a mock message handler that executes function argument to validate the request and mock a response.
        /// </summary>
        private HttpClient CreateHttpClientWithMockHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> valueFunction)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(valueFunction)
                .Verifiable();

            return new HttpClient(mockHttpMessageHandler.Object);
        }

        /// <summary>
        /// A simple type for testing InvokeResponse body.
        /// </summary>
        internal class TestContentBody
        {
            public TestContentBody(string id, string someProp)
            {
                Id = id;
                SomeProp = someProp;
            }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("someProp")]
            public string SomeProp { get; set; }
        }
    }
}
