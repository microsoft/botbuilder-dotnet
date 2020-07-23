// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests.Skills
{
    public class SkillHttpClientTests
    {
        [Fact]
        public async Task PostActivityWithOriginatingAudience()
        {
            var conversationId = Guid.NewGuid().ToString();
            var conversationIdFactory = new SimpleConversationIdFactory(conversationId);
            var testActivity = MessageFactory.Text("some message");
            testActivity.Conversation = new ConversationAccount();
            var skill = new BotFrameworkSkill
            {
                Id = "SomeSkill",
                AppId = string.Empty,
                SkillEndpoint = new Uri("https://someskill.com/api/messages")
            };

            var httpClient = CreateHttpClientWithMockHandler((request, cancellationToken) =>
            {
                Assert.Equal(skill.SkillEndpoint, request.RequestUri);

                // Assert that the activity being sent has what we expect.
                var activitySent = JsonConvert.DeserializeObject<Activity>(request.Content.ReadAsStringAsync().Result);
                Assert.Equal(conversationId, activitySent.Conversation.Id);
                Assert.Equal("https://parentbot.com/api/messages", activitySent.ServiceUrl);

                // Create mock response.
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var sut = new SkillHttpClient(httpClient, new Mock<ICredentialProvider>().Object, conversationIdFactory);
            var result = await sut.PostActivityAsync<object>("someOriginatingAudience", string.Empty, skill, new Uri("https://parentbot.com/api/messages"), testActivity, CancellationToken.None);

            // Assert factory options
            Assert.Equal(string.Empty, conversationIdFactory.CreationOptions.FromBotId);
            Assert.Equal("someOriginatingAudience", conversationIdFactory.CreationOptions.FromBotOAuthScope);
            Assert.Equal(testActivity, conversationIdFactory.CreationOptions.Activity);
            Assert.Equal(skill, conversationIdFactory.CreationOptions.BotFrameworkSkill);

            // Assert result
            Assert.IsType<InvokeResponse<object>>(result);
            Assert.Equal(200, result.Status);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PostActivityUsingInvokeResponse(bool isGovernment)
        {
            var conversationId = Guid.NewGuid().ToString();
            var conversationIdFactory = new SimpleConversationIdFactory(conversationId);
            var testActivity = MessageFactory.Text("some message");
            testActivity.Conversation = new ConversationAccount();
            var expectedOAuthScope = AuthenticationConstants.ToChannelFromBotOAuthScope;
            var mockChannelProvider = new Mock<IChannelProvider>();
            mockChannelProvider.Setup(m => m.IsGovernment())
                .Returns(() =>
                {
                    if (isGovernment)
                    {
                        expectedOAuthScope = GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope;
                    }

                    return isGovernment;
                });

            var skill = new BotFrameworkSkill
            {
                Id = "SomeSkill",
                AppId = string.Empty,
                SkillEndpoint = new Uri("https://someskill.com/api/messages")
            };

            var httpClient = CreateHttpClientWithMockHandler((request, cancellationToken) =>
            {
                Assert.Equal(skill.SkillEndpoint, request.RequestUri);

                // Assert that the activity being sent has what we expect.
                var activitySent = JsonConvert.DeserializeObject<Activity>(request.Content.ReadAsStringAsync().Result);
                Assert.Equal(conversationId, activitySent.Conversation.Id);
                Assert.Equal("https://parentbot.com/api/messages", activitySent.ServiceUrl);

                // Create mock response.
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                return Task.FromResult(response);
            });

            var sut = new SkillHttpClient(httpClient, new Mock<ICredentialProvider>().Object, conversationIdFactory, mockChannelProvider.Object);
            var result = await sut.PostActivityAsync(string.Empty, skill, new Uri("https://parentbot.com/api/messages"), testActivity, CancellationToken.None);

            // Assert factory options
            Assert.Equal(string.Empty, conversationIdFactory.CreationOptions.FromBotId);
            Assert.Equal(expectedOAuthScope, conversationIdFactory.CreationOptions.FromBotOAuthScope);
            Assert.Equal(testActivity, conversationIdFactory.CreationOptions.Activity);
            Assert.Equal(skill, conversationIdFactory.CreationOptions.BotFrameworkSkill);

            // Assert result
            Assert.IsType<InvokeResponse<object>>(result);
            Assert.Equal(200, result.Status);
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

        private class SimpleConversationIdFactory : SkillConversationIdFactoryBase
        {
            private readonly string _conversationId;
            private readonly ConcurrentDictionary<string, SkillConversationReference> _conversationRefs = new ConcurrentDictionary<string, SkillConversationReference>();

            public SimpleConversationIdFactory(string conversationId)
            {
                _conversationId = conversationId;
            }

            // Public property to capture and assert the options passed to <see cref="CreateSkillConversationIdAsync"/>.
            public SkillConversationIdFactoryOptions CreationOptions { get; private set; }

            public override Task<string> CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions options, CancellationToken cancellationToken)
            {
                CreationOptions = options;

                var key = _conversationId;
                _conversationRefs.GetOrAdd(key, new SkillConversationReference
                {
                    ConversationReference = options.Activity.GetConversationReference(),
                    OAuthScope = options.FromBotOAuthScope
                });
                return Task.FromResult(key);
            }

            public override Task<SkillConversationReference> GetSkillConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                return Task.FromResult(_conversationRefs[skillConversationId]);
            }

            public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
