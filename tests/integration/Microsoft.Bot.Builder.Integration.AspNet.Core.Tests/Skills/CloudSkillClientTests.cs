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
    public class CloudSkillClientTests
    {
        [Fact]
        public async Task PostActivityToSkillTest()
        {
            // Arrange
            var conversationId = Guid.NewGuid().ToString();
            var conversationIdFactory = new SimpleConversationIdFactory(conversationId);
            var activity = MessageFactory.Text("some message");
            activity.Conversation = new ConversationAccount();
            var skill = new BotFrameworkSkill
            {
                Id = "SomeSkill",
                AppId = string.Empty,
                SkillEndpoint = new Uri("https://someskill.com/api/messages")
            };

            var client = CreateHttpClientWithMockHandler((request, cancellationToken) =>
            {
                Assert.Equal(skill.SkillEndpoint, request.RequestUri);

                // Assert that the activity being sent has what we expect.
                var activitySent = JsonConvert.DeserializeObject<Activity>(request.Content.ReadAsStringAsync().Result);
                Assert.Equal(conversationId, activitySent.Conversation.Id);
                Assert.Equal("https://parentbot.com/api/messages", activitySent.ServiceUrl);

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            var auth = new Mock<BotFrameworkAuthentication>();
            auth.Setup(a => a.GetAppCredentialsAsync(It.IsAny<string>(), It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new MicrosoftAppCredentials(string.Empty, string.Empty) as AppCredentials));
            auth.Setup(a => a.GetOriginatingAudience()).Returns(AuthenticationConstants.ToChannelFromBotOAuthScope);

            // Act
            var sut = new CloudSkillClient(client, auth.Object, conversationIdFactory);
            var response = await sut.PostActivityAsync(string.Empty, skill, new Uri("https://parentbot.com/api/messages"), activity, CancellationToken.None);

            // Assert
            Assert.Equal(string.Empty, conversationIdFactory.CreationOptions.FromBotId);
            Assert.Equal(AuthenticationConstants.ToChannelFromBotOAuthScope, conversationIdFactory.CreationOptions.FromBotOAuthScope);
            Assert.Equal(activity, conversationIdFactory.CreationOptions.Activity);
            Assert.Equal(skill, conversationIdFactory.CreationOptions.BotFrameworkSkill);

            Assert.IsType<InvokeResponse<object>>(response);
            Assert.Equal(200, response.Status);
        }

        private HttpClient CreateHttpClientWithMockHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> requestProcessor)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(requestProcessor)
                .Verifiable();

            return new HttpClient(handler.Object);
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
