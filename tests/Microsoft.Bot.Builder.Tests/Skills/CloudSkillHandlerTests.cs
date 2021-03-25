// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Tests.Skills
{
    public class CloudSkillHandlerTests
    {
        private static readonly string TestBotId = Guid.NewGuid().ToString("N");
        private static readonly string TestBotEndpoint = "http://testbot.com/api/messages";

        private static readonly string TestSkillId = Guid.NewGuid().ToString("N");
        private static readonly string TestSkillEndpoint = "http://testskill.com/api/messages";

        private static readonly string TestAuthHeader = string.Empty; // Empty since claims extraction is being mocked
        private static readonly string TestActivityId = Guid.NewGuid().ToString("N");

        [Theory]
        [InlineData(ActivityTypes.EndOfConversation)]
        [InlineData(ActivityTypes.Event)]
        [InlineData(ActivityTypes.Message)]
        public async Task TestSendToConversationAsync(string activityType)
        {
            // Arrange
            var activity = new Activity(activityType);
            var conversationIdFactory = new TestSkillConversationIdFactory();
            string conversationId = await CreateTestSkillConversationIdAsync(conversationIdFactory, activity);

            var adapter = new Mock<BotAdapter>();
            adapter.Setup(a => a.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (token, conv, audience, callback, cancel) =>
                {
                    var turn = new TurnContext(adapter.Object, conv.GetContinuationActivity());
                    await callback(turn, cancel);

                    // Assert the callback set the right properties.
                    Assert.Equal($"{CallerIdConstants.BotToBotPrefix}{TestSkillId}", turn.Activity.CallerId);
                });
            adapter.Setup(a => a.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()))
                .Callback<ITurnContext, Activity[], CancellationToken>((turn, activities, cancel) =>
                {
                    // Messages should not have a caller id set when sent back to the caller.
                    Assert.Null(activities[0].CallerId);
                    Assert.Null(activities[0].ReplyToId);

                    // Do nothing, we don't want the activities sent to the channel in the tests.
                })
                .Returns(Task.FromResult(new[] { new ResourceResponse("resourceId") }));

            var bot = new Mock<IBot>();
            var auth = new Mock<BotFrameworkAuthentication>();
            auth.Setup(a => a.AuthenticateChannelRequestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>(AuthenticateChannelRequest);

            // Act
            var sut = new CloudSkillHandler(adapter.Object, bot.Object, conversationIdFactory, auth.Object);
            var response = await sut.HandleSendToConversationAsync(TestAuthHeader, conversationId, activity);

            // Assert
            adapter.Verify(a => a.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            if (activityType == ActivityTypes.Message)
            {
                // Assert mock SendActivitiesAsync was called
                adapter.Verify(a => a.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
                Assert.Equal("resourceId", response.Id);
            }
            else
            {
                // Assert mock SendActivitiesAsync was not called
                adapter.Verify(a => a.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Theory]
        [InlineData(ActivityTypes.Message)]
        [InlineData(ActivityTypes.Event)]
        [InlineData(ActivityTypes.EndOfConversation)]
        public async Task TestReplyToActivityAsync(string activityType)
        {
            // Arrange
            var activity = new Activity(activityType);
            var conversationIdFactory = new TestSkillConversationIdFactory();
            string conversationId = await CreateTestSkillConversationIdAsync(conversationIdFactory, activity);

            var adapter = new Mock<BotAdapter>();
            adapter.Setup(a => a.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (token, conv, audience, callback, cancel) =>
                {
                    var turn = new TurnContext(adapter.Object, conv.GetContinuationActivity());
                    await callback(turn, cancel);

                    // Assert the callback set the right properties.
                    Assert.Equal($"{CallerIdConstants.BotToBotPrefix}{TestSkillId}", turn.Activity.CallerId);
                });
            adapter.Setup(a => a.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()))
                .Callback<ITurnContext, Activity[], CancellationToken>((turn, activities, cancel) =>
                {
                    // Messages should not have a caller id set when sent back to the caller.
                    Assert.Null(activities[0].CallerId);
                    Assert.Equal(TestActivityId, activities[0].ReplyToId);

                    // Do nothing, we don't want the activities sent to the channel in the tests.
                })
                .Returns(Task.FromResult(new[] { new ResourceResponse("resourceId") }));

            var bot = new Mock<IBot>();
            var auth = new Mock<BotFrameworkAuthentication>();
            auth.Setup(a => a.AuthenticateChannelRequestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>(AuthenticateChannelRequest);

            // Act
            var sut = new CloudSkillHandler(adapter.Object, bot.Object, conversationIdFactory, auth.Object);
            var response = await sut.HandleReplyToActivityAsync(TestAuthHeader, conversationId, TestActivityId, activity);

            // Assert
            adapter.Verify(a => a.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            if (activityType == ActivityTypes.Message)
            {
                // Assert mock SendActivitiesAsync was called
                adapter.Verify(a => a.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
                Assert.Equal("resourceId", response.Id);
            }
            else
            {
                // Assert mock SendActivitiesAsync was not called
                adapter.Verify(a => a.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public async Task TestDeleteActivityAsync()
        {
            // Arrange
            var activity = (Activity)Activity.CreateMessageActivity();
            var conversationIdFactory = new TestSkillConversationIdFactory();
            string conversationId = await CreateTestSkillConversationIdAsync(conversationIdFactory, activity);

            var adapter = new Mock<BotAdapter>();
            adapter.Setup(a => a.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (token, conv, audience, callback, cancel) =>
                {
                    var turn = new TurnContext(adapter.Object, conv.GetContinuationActivity());
                    await callback(turn, cancel);
                });
            adapter.Setup(a => a.DeleteActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<ConversationReference>(), It.IsAny<CancellationToken>()))
                .Callback<ITurnContext, ConversationReference, CancellationToken>((turn, conv, cancel) =>
                {
                    Assert.Equal(TestActivityId, conv.ActivityId);
                });

            var bot = new Mock<IBot>();
            var auth = new Mock<BotFrameworkAuthentication>();
            auth.Setup(a => a.AuthenticateChannelRequestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>(AuthenticateChannelRequest);

            // Act
            var sut = new CloudSkillHandler(adapter.Object, bot.Object, conversationIdFactory, auth.Object);
            await sut.HandleDeleteActivityAsync(TestAuthHeader, conversationId, TestActivityId);

            // Assert
            adapter.Verify(a => a.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            adapter.Verify(a => a.DeleteActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<ConversationReference>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        [Fact]
        public async void TestUpdateActivityAsync()
        {
            // Arrange
            var activity = (Activity)Activity.CreateMessageActivity();
            var message = activity.Text = $"TestUpdate {DateTime.Now}.";
            var conversationIdFactory = new TestSkillConversationIdFactory();
            string conversationId = await CreateTestSkillConversationIdAsync(conversationIdFactory, activity);

            var adapter = new Mock<BotAdapter>();
            adapter.Setup(a => a.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (token, conv, audience, callback, cancel) =>
                {
                    var turn = new TurnContext(adapter.Object, conv.GetContinuationActivity());
                    await callback(turn, cancel);

                    // Assert the callback set the right properties.
                    Assert.Equal($"{CallerIdConstants.BotToBotPrefix}{TestSkillId}", turn.Activity.CallerId);
                });
            adapter.Setup(a => a.UpdateActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Callback<ITurnContext, Activity, CancellationToken>((turn, newActivity, cancel) =>
                {
                    Assert.Equal(TestActivityId, newActivity.ReplyToId);
                    Assert.Equal(message, newActivity.Text);
                })
                .Returns(Task.FromResult(new ResourceResponse("resourceId")));

            var bot = new Mock<IBot>();
            var auth = new Mock<BotFrameworkAuthentication>();
            auth.Setup(a => a.AuthenticateChannelRequestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>(AuthenticateChannelRequest);

            // Act
            var sut = new CloudSkillHandler(adapter.Object, bot.Object, conversationIdFactory, auth.Object);
            ResourceResponse response = await sut.HandleUpdateActivityAsync(TestAuthHeader, conversationId, TestActivityId, activity);

            // Assert
            adapter.Verify(a => a.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            adapter.Verify(a => a.UpdateActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            Assert.Equal("resourceId", response.Id);
        }

        private async Task<string> CreateTestSkillConversationIdAsync(SkillConversationIdFactoryBase conversationIdFactory, Activity activity)
        {
            activity.ApplyConversationReference(new ConversationReference
            {
                Conversation = new ConversationAccount(id: TestBotId),
                ServiceUrl = TestBotEndpoint
            });

            var skill = new BotFrameworkSkill
            {
                AppId = TestSkillId,
                Id = "skill",
                SkillEndpoint = new Uri(TestSkillEndpoint)
            };

            var options = new SkillConversationIdFactoryOptions
            {
                FromBotOAuthScope = TestBotId,
                FromBotId = TestBotId,
                Activity = activity,
                BotFrameworkSkill = skill
            };

            return await conversationIdFactory.CreateSkillConversationIdAsync(options, CancellationToken.None);
        }

        private Task<ClaimsIdentity> AuthenticateChannelRequest(string authHeader, CancellationToken cancellationToken)
        {
            var token = new ClaimsIdentity();

            token.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, TestBotId));
            token.AddClaim(new Claim(AuthenticationConstants.AppIdClaim, TestSkillId));
            token.AddClaim(new Claim(AuthenticationConstants.ServiceUrlClaim, TestBotEndpoint));

            return Task.FromResult(token);
        }

        private class TestSkillConversationIdFactory : SkillConversationIdFactoryBase
        {
            private readonly ConcurrentDictionary<string, string> _conversationRefs = new ConcurrentDictionary<string, string>();

            public override Task<string> CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions options, CancellationToken cancellationToken)
            {
                var skillConversationReference = new SkillConversationReference
                {
                    ConversationReference = options.Activity.GetConversationReference(),
                    OAuthScope = options.FromBotOAuthScope
                };
                var key = $"{options.FromBotId}-{options.BotFrameworkSkill.AppId}-{skillConversationReference.ConversationReference.Conversation.Id}-{skillConversationReference.ConversationReference.ChannelId}-skillconvo";
                _conversationRefs.GetOrAdd(key, JsonConvert.SerializeObject(skillConversationReference));
                return Task.FromResult(key);
            }

            public override Task<SkillConversationReference> GetSkillConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                var conversationReference = JsonConvert.DeserializeObject<SkillConversationReference>(_conversationRefs[skillConversationId]);
                return Task.FromResult(conversationReference);
            }

            public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                _conversationRefs.TryRemove(skillConversationId, out _);
                return Task.CompletedTask;
            }
        }
    }
}
