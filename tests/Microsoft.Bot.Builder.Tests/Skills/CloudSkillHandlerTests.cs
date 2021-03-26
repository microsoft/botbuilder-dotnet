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
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Tests.Skills
{
    public class CloudSkillHandlerTests
    {
        private static readonly string TestSkillId = Guid.NewGuid().ToString("N");
        private static readonly string TestAuthHeader = string.Empty; // Empty since claims extraction is being mocked

        [Theory]
        [InlineData(ActivityTypes.Message, null)]
        [InlineData(ActivityTypes.Message, "replyToId")]
        [InlineData(ActivityTypes.Event, null)]
        [InlineData(ActivityTypes.Event, "replyToId")]
        [InlineData(ActivityTypes.EndOfConversation, null)]
        [InlineData(ActivityTypes.EndOfConversation, "replyToId")]
        public async Task TestSendAndReplyToConversationAsync(string activityType, string replyToId)
        {
            // Arrange
            var mockObjects = new CloudSkillHandlerTestMocks();
            var activity = new Activity(activityType) { ReplyToId = replyToId };
            var conversationId = await mockObjects.CreateAndApplyConversationIdAsync(activity);

            // Act
            var sut = new CloudSkillHandler(mockObjects.Adapter.Object, mockObjects.Bot.Object, mockObjects.ConversationIdFactory, mockObjects.Auth.Object);
            var response = replyToId == null ? await sut.HandleSendToConversationAsync(TestAuthHeader, conversationId, activity) : await sut.HandleReplyToActivityAsync(TestAuthHeader, conversationId, replyToId, activity);

            // Assert
            // Assert the turnContext.
            Assert.Equal($"{CallerIdConstants.BotToBotPrefix}{TestSkillId}", mockObjects.TurnContext.Activity.CallerId);
            Assert.NotNull(mockObjects.TurnContext.TurnState.Get<SkillConversationReference>(CloudSkillHandler.SkillConversationReferenceKey));

            // Assert based on activity type,
            if (activityType == ActivityTypes.Message)
            {
                // Should be sent to the channel and not to the bot.
                Assert.NotNull(mockObjects.ChannelActivity);
                Assert.Null(mockObjects.BotActivity);

                // We should get the resourceId returned by the mock.
                Assert.Equal("resourceId", response.Id);

                // Assert the activity sent to the channel.
                Assert.Equal(activityType, mockObjects.ChannelActivity.Type);
                Assert.Null(mockObjects.ChannelActivity.CallerId);
                Assert.Equal(replyToId, mockObjects.ChannelActivity.ReplyToId);
            }
            else
            {
                // Should be sent to the bot and not to the channel.
                Assert.Null(mockObjects.ChannelActivity);
                Assert.NotNull(mockObjects.BotActivity);

                // If the activity is bounced back to the bot we will get a GUID and not the mocked resourceId.
                Assert.NotEqual("resourceId", response.Id);

                // Assert the activity sent back to the bot.
                Assert.Equal(activityType, mockObjects.BotActivity.Type);
                Assert.Equal(replyToId, mockObjects.BotActivity.ReplyToId);
            }
        }

        [Theory]
        [InlineData(ActivityTypes.Command, "application/myApplicationCommand", null)]
        [InlineData(ActivityTypes.Command, "application/myApplicationCommand", "replyToId")]
        [InlineData(ActivityTypes.Command, "other/myBotCommand", null)]
        [InlineData(ActivityTypes.Command, "other/myBotCommand", "replyToId")]
        [InlineData(ActivityTypes.CommandResult, "application/myApplicationCommandResult", null)]
        [InlineData(ActivityTypes.CommandResult, "application/myApplicationCommandResult", "replyToId")]
        [InlineData(ActivityTypes.CommandResult, "other/myBotCommand", null)]
        [InlineData(ActivityTypes.CommandResult, "other/myBotCommand", "replyToId")]
        public async Task TestCommandActivities(string commandActivityType, string name, string replyToId)
        {
            // Arrange
            var mockObjects = new CloudSkillHandlerTestMocks();
            var activity = new Activity(commandActivityType) { Name = name, ReplyToId = replyToId };
            var conversationId = await mockObjects.CreateAndApplyConversationIdAsync(activity);

            // Act
            var sut = new CloudSkillHandler(mockObjects.Adapter.Object, mockObjects.Bot.Object, mockObjects.ConversationIdFactory, mockObjects.Auth.Object);
            var response = replyToId == null ? await sut.HandleSendToConversationAsync(TestAuthHeader, conversationId, activity) : await sut.HandleReplyToActivityAsync(TestAuthHeader, conversationId, replyToId, activity);

            // Assert
            // Assert the turnContext.
            Assert.Equal($"{CallerIdConstants.BotToBotPrefix}{TestSkillId}", mockObjects.TurnContext.Activity.CallerId);
            Assert.NotNull(mockObjects.TurnContext.TurnState.Get<SkillConversationReference>(CloudSkillHandler.SkillConversationReferenceKey));
            if (name.StartsWith("application/"))
            {
                // Should be sent to the channel and not to the bot.
                Assert.NotNull(mockObjects.ChannelActivity);
                Assert.Null(mockObjects.BotActivity);

                // We should get the resourceId returned by the mock.
                Assert.Equal("resourceId", response.Id);
            }
            else
            {
                // Should be sent to the bot and not to the channel.
                Assert.Null(mockObjects.ChannelActivity);
                Assert.NotNull(mockObjects.BotActivity);

                // If the activity is bounced back to the bot we will get a GUID and not the mocked resourceId.
                Assert.NotEqual("resourceId", response.Id);
            }
        }

        [Fact]
        public async Task TestDeleteActivityAsync()
        {
            // Arrange
            var mockObjects = new CloudSkillHandlerTestMocks();
            var activity = new Activity(ActivityTypes.Message);
            var conversationId = await mockObjects.CreateAndApplyConversationIdAsync(activity);
            var activityToDelete = Guid.NewGuid().ToString();

            // Act
            var sut = new CloudSkillHandler(mockObjects.Adapter.Object, mockObjects.Bot.Object, mockObjects.ConversationIdFactory, mockObjects.Auth.Object);
            await sut.HandleDeleteActivityAsync(TestAuthHeader, conversationId, activityToDelete);

            // Assert
            Assert.NotNull(mockObjects.TurnContext.TurnState.Get<SkillConversationReference>(CloudSkillHandler.SkillConversationReferenceKey));
            Assert.Equal(activityToDelete, mockObjects.ActivityIdToDelete);
        }

        [Fact]
        public async void TestUpdateActivityAsync()
        {
            // Arrange
            var mockObjects = new CloudSkillHandlerTestMocks();
            var activity = new Activity(ActivityTypes.Message) { Text = $"TestUpdate {DateTime.Now}." };
            var conversationId = await mockObjects.CreateAndApplyConversationIdAsync(activity);
            var activityToUpdate = Guid.NewGuid().ToString();

            // Act
            var sut = new CloudSkillHandler(mockObjects.Adapter.Object, mockObjects.Bot.Object, mockObjects.ConversationIdFactory, mockObjects.Auth.Object);
            var response = await sut.HandleUpdateActivityAsync(TestAuthHeader, conversationId, activityToUpdate, activity);

            // Assert
            Assert.Equal("resourceId", response.Id);
            Assert.NotNull(mockObjects.TurnContext.TurnState.Get<SkillConversationReference>(CloudSkillHandler.SkillConversationReferenceKey));
            Assert.Equal(activityToUpdate, mockObjects.TurnContext.Activity.Id);
            Assert.Equal(activity.Text, mockObjects.UpdateActivity.Text);
        }

        /// <summary>
        /// Helper class with mocks for adapter, bot and auth needed to instantiate CloudSkillHandler and run tests.
        /// This class also captures the turnContext and activities sent back to the bot and the channel so we can run asserts on them.
        /// </summary>
        private class CloudSkillHandlerTestMocks
        {
            private static readonly string TestBotId = Guid.NewGuid().ToString("N");
            private static readonly string TestBotEndpoint = "http://testbot.com/api/messages";

            private static readonly string TestSkillEndpoint = "http://testskill.com/api/messages";

            public CloudSkillHandlerTestMocks()
            {
                Adapter = CreateMockAdapter();
                Auth = CreateMockBotFrameworkAuthentication();
                Bot = CreateMockBot();
                ConversationIdFactory = new TestSkillConversationIdFactory();
            }

            public SkillConversationIdFactoryBase ConversationIdFactory { get; }

            public Mock<BotAdapter> Adapter { get; }

            public Mock<BotFrameworkAuthentication> Auth { get;  }

            public Mock<IBot> Bot { get;  }

            // Gets the TurnContext created to call the bot.
            public TurnContext TurnContext { get; private set; }
            
            /// <summary>
            /// Gets the Activity sent to the channel.
            /// </summary>
            public Activity ChannelActivity { get; private set; }

            /// <summary>
            /// Gets the Activity sent to the Bot.
            /// </summary>
            public Activity BotActivity { get; private set; }

            /// <summary>
            /// Gets the update activity.
            /// </summary>
            public Activity UpdateActivity { get; private set; }

            /// <summary>
            /// Gets the Activity sent to the Bot.
            /// </summary>
            public string ActivityIdToDelete { get; private set; }

            public async Task<string> CreateAndApplyConversationIdAsync(Activity activity)
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

                return await ConversationIdFactory.CreateSkillConversationIdAsync(options, CancellationToken.None);
            }

            private Mock<BotAdapter> CreateMockAdapter()
            {
                var adapter = new Mock<BotAdapter>();

                // Mock the adapter ContinueConversationAsync method
                // This code block catches and executes the custom bot callback created by the service handler.
                adapter.Setup(a => a.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                    .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (token, conv, audience, botCallbackHandler, cancel) =>
                    {
                        // Create and capture the TurnContext so we can run assertions on it.
                        TurnContext = new TurnContext(adapter.Object, conv.GetContinuationActivity());
                        await botCallbackHandler(TurnContext, cancel);
                    });

                // Mock the adapter SendActivitiesAsync method (this for the cases where activity is sent back to the parent or channel)
                adapter.Setup(a => a.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()))
                    .Callback<ITurnContext, Activity[], CancellationToken>((turn, activities, cancel) =>
                    {
                        // Capture the activity sent to the channel
                        ChannelActivity = activities[0];

                        // Do nothing, we don't want the activities sent to the channel in the tests.
                    })
                    .Returns(Task.FromResult(new[]
                    {
                        // Return a well known resourceId so we can assert we capture the right return value.
                        new ResourceResponse("resourceId")
                    }));

                // Mock the DeleteActivityAsync method
                adapter.Setup(a => a.DeleteActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<ConversationReference>(), It.IsAny<CancellationToken>()))
                    .Callback<ITurnContext, ConversationReference, CancellationToken>((turn, conv, cancel) =>
                    {
                        // Capture the activity id to delete so we can assert it. 
                        ActivityIdToDelete = conv.ActivityId;
                    });

                // Mock the UpdateActivityAsync method
                adapter.Setup(a => a.UpdateActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                    .Callback<ITurnContext, Activity, CancellationToken>((turn, newActivity, cancel) =>
                    {
                        // Capture the activity to update.
                        UpdateActivity = newActivity;
                    })
                    .Returns(Task.FromResult(new ResourceResponse("resourceId")));

                return adapter;
            }

            private Mock<IBot> CreateMockBot()
            {
                var bot = new Mock<IBot>();
                bot.Setup(b => b.OnTurnAsync(It.IsAny<ITurnContext>(), It.IsAny<CancellationToken>()))
                    .Callback<ITurnContext, CancellationToken>((turnContext, ct) =>
                    {
                        BotActivity = turnContext.Activity;
                    });
                return bot;
            }

            private Mock<BotFrameworkAuthentication> CreateMockBotFrameworkAuthentication()
            {
                var auth = new Mock<BotFrameworkAuthentication>();
                auth.Setup(a => a.AuthenticateChannelRequestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns<string, CancellationToken>((authHeader, cancellationToken) =>
                    {
                        var claimsIdentity = new ClaimsIdentity();

                        claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, TestBotId));
                        claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AppIdClaim, TestSkillId));
                        claimsIdentity.AddClaim(new Claim(AuthenticationConstants.ServiceUrlClaim, TestBotEndpoint));

                        return Task.FromResult(claimsIdentity);
                    });
                return auth;
            }
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
