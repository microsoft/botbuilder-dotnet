// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
    public class SkillHandlerTests
    {
        private readonly string _botId = Guid.NewGuid().ToString("N");
        private readonly ClaimsIdentity _claimsIdentity;
        private readonly string _conversationId;
        private readonly ConversationReference _conversationReference;
        private readonly Mock<BotAdapter> _mockAdapter = new Mock<BotAdapter>();
        private readonly Mock<IBot> _mockBot = new Mock<IBot>();
        private readonly string _skillId = Guid.NewGuid().ToString("N");
        private readonly TestConversationIdFactory _testConversationIdFactory = new TestConversationIdFactory();

        public SkillHandlerTests()
        {
            _claimsIdentity = new ClaimsIdentity();
            _claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, _botId));
            _claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AppIdClaim, _skillId));
            _claimsIdentity.AddClaim(new Claim(AuthenticationConstants.ServiceUrlClaim, "http://testbot.com/api/messages"));
            _conversationReference = new ConversationReference
            {
                Conversation = new ConversationAccount(id: Guid.NewGuid().ToString("N")),
                ServiceUrl = "http://testbot.com/api/messages"
            };

            var activity = (Activity)Activity.CreateMessageActivity();
            activity.ApplyConversationReference(_conversationReference);
            var skill = new BotFrameworkSkill
            {
                AppId = _skillId,
                Id = "skill",
                SkillEndpoint = new Uri("http://testbot.com/api/messages")
            };

            var options = new SkillConversationIdFactoryOptions
            {
                FromBotOAuthScope = _botId,
                FromBotId = _botId,
                Activity = activity,
                BotFrameworkSkill = skill
            };

            _conversationId = _testConversationIdFactory.CreateSkillConversationIdAsync(options, CancellationToken.None).Result;
        }

        [Fact]
        public async Task LegacyConversationIdFactoryWorksTest()
        {
            var legacyFactory = new TestLegacyConversationIdFactory();
            var conversationReference = new ConversationReference
            {
                Conversation = new ConversationAccount(id: Guid.NewGuid().ToString("N")),
                ServiceUrl = "http://testbot.com/api/messages"
            };

            // Testing the deprecated method for backward compatibility.
#pragma warning disable 612
            var conversationId = await legacyFactory.CreateSkillConversationIdAsync(conversationReference, CancellationToken.None);
#pragma warning restore 612
            _mockAdapter.Setup(x => x.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (identity, reference, audience, callback, cancellationToken) =>
                {
                    // Invoke the callback created by the handler so we can assert the rest of the execution. 
                    var turnContext = new TurnContext(_mockAdapter.Object, _conversationReference.GetContinuationActivity());
                    await callback(turnContext, cancellationToken);
                });

            var activity = Activity.CreateMessageActivity();
            activity.ApplyConversationReference(conversationReference);

            var sut = CreateSkillHandlerForTesting(legacyFactory);

            await sut.TestOnSendToConversationAsync(_claimsIdentity, conversationId, (Activity)activity, CancellationToken.None);
        }

        [Theory]
        [InlineData(ActivityTypes.EndOfConversation)]
        [InlineData(ActivityTypes.Event)]
        [InlineData(ActivityTypes.Message)]
        public async Task OnSendToConversationAsyncTest(string activityType)
        {            
            var activity = new Activity(activityType);

            _mockAdapter.Setup(x => x.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (identity, reference, audience, callback, cancellationToken) =>
                {
                    // Invoke the callback created by the handler so we can assert the rest of the execution. 
                    var turnContext = new TurnContext(_mockAdapter.Object, _conversationReference.GetContinuationActivity());
                    await callback(turnContext, cancellationToken);

                    // Assert the callback set the right properties.
                    Assert.Equal($"{CallerIdConstants.BotToBotPrefix}{_skillId}", turnContext.Activity.CallerId);
                });

            _mockAdapter.Setup(x => x.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()))
                .Callback<ITurnContext, Activity[], CancellationToken>((turnContext, activities, cancellationToken) =>
                {
                    // Messages should not have a caller id set when sent back to the caller.
                    Assert.Null(activities[0].CallerId);
                    Assert.Null(activities[0].ReplyToId);
                })
                .Returns(Task.FromResult(new[] { new ResourceResponse { Id = "resourceId" } }));

            var sut = CreateSkillHandlerForTesting();
            var resourceResponse = await sut.TestOnSendToConversationAsync(_claimsIdentity, _conversationId, activity, CancellationToken.None);

            if (activityType == ActivityTypes.Message)
            {
                // Assert mock SendActivitiesAsync was called
                _mockAdapter.Verify(ma => ma.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

                Assert.Equal("resourceId", resourceResponse.Id);
            }
        }

        [Theory]
        [InlineData(ActivityTypes.EndOfConversation)]
        [InlineData(ActivityTypes.Event)]
        [InlineData(ActivityTypes.Message)]
        public async Task OnOnReplyToActivityAsyncTest(string activityType)
        {
            var activity = new Activity(activityType);

            // Mock ContinueConversationAsync (this is called at the end of ProcessActivityAsync). 
            _mockAdapter.Setup(x => x.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (identity, reference, audience, callback, cancellationToken) =>
                {
                    // Invoke the callback created by the handler so we can assert the rest of the execution. 
                    var turnContext = new TurnContext(_mockAdapter.Object, _conversationReference.GetContinuationActivity());
                    await callback(turnContext, cancellationToken);

                    // Assert the callback set the right properties.
                    Assert.Equal($"{CallerIdConstants.BotToBotPrefix}{_skillId}", turnContext.Activity.CallerId);
                });

            var replyToActivityId = Guid.NewGuid().ToString("N");

            // Mock SendActivitiesAsync, assert values sent and return an arbitrary ResourceResponse.
            _mockAdapter.Setup(x => x.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()))
                .Callback<ITurnContext, Activity[], CancellationToken>((turnContext, activities, cancellationToken) =>
                {
                    // Messages should not have a caller id set when sent back to the caller.
                    Assert.Null(activities[0].CallerId);
                    Assert.Equal(replyToActivityId, activities[0].ReplyToId);

                    // Do nothing, we don't want the activities sent to the channel in the tests.
                })
                .Returns(Task.FromResult(new[] { new ResourceResponse { Id = "resourceId" } }));

            // Call TestOnReplyToActivity on our helper so it calls the OnReply method on the handler and executes our mocks. 
            var sut = CreateSkillHandlerForTesting();
            var resourceResponse = await sut.TestOnReplyToActivityAsync(_claimsIdentity, _conversationId, replyToActivityId, activity, CancellationToken.None);

            // Assert mock ContinueConversationAsync was called
            _mockAdapter.Verify(ma => ma.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            
            if (activityType == ActivityTypes.Message)
            {
                // Assert mock SendActivitiesAsync was called
                _mockAdapter.Verify(ma => ma.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

                Assert.Equal("resourceId", resourceResponse.Id);
            }
            else
            {
                // Assert mock SendActivitiesAsync wasn't called
                _mockAdapter.Verify(ma => ma.SendActivitiesAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity[]>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public async Task OnUpdateActivityAsyncTest()
        {
            // Mock ContinueConversationAsync (this is called at the end of OnUpdateActivityAsync). 
            _mockAdapter.Setup(x => x.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (identity, reference, audience, callback, cancellationToken) =>
                {
                    // Create a TurnContext with our mock adapter. 
                    var turnContext = new TurnContext(_mockAdapter.Object, _conversationReference.GetContinuationActivity());
                    
                    // Execute the callback with our turnContext.
                    await callback(turnContext, cancellationToken);

                    // Assert the callback set the right properties.
                    Assert.Equal($"{CallerIdConstants.BotToBotPrefix}{_skillId}", turnContext.Activity.CallerId);
                });

            var activity = Activity.CreateMessageActivity();
            var message = activity.Text = $"TestUpdate {DateTime.Now}.";
            var activityId = Guid.NewGuid().ToString("N");

            // Mock UpdateActivityAsync, assert the activity being sent and return and arbitrary ResourceResponse.
            _mockAdapter.Setup(x => x.UpdateActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                .Callback<ITurnContext, Activity, CancellationToken>((context, newActivity, cancellationToken) =>
                {
                    // Assert the activity being sent.
                    Assert.Equal(activityId, newActivity.ReplyToId);
                    Assert.Equal(message, newActivity.Text);
                })
                .Returns(Task.FromResult(new ResourceResponse { Id = "resourceId" }));

            var sut = CreateSkillHandlerForTesting();
            var resourceResponse = await sut.TestOnUpdateActivityAsync(_claimsIdentity, _conversationId, activityId, (Activity)activity, CancellationToken.None);

            // Assert mock methods were called
            _mockAdapter.Verify(ma => ma.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            _mockAdapter.Verify(ma => ma.UpdateActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

            Assert.Equal("resourceId", resourceResponse.Id);
        }

        [Fact]
        public async Task OnDeleteActivityAsyncTest()
        {
            // Mock ContinueConversationAsync (this is called at the end of OnUpdateActivityAsync). 
            _mockAdapter.Setup(x => x.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, string, BotCallbackHandler, CancellationToken>(async (identity, reference, audience, callback, cancellationToken) =>
                {
                    // Create a TurnContext with our mock adapter. 
                    var turnContext = new TurnContext(_mockAdapter.Object, _conversationReference.GetContinuationActivity());
                    
                    // Execute the callback with our turnContext.
                    await callback(turnContext, cancellationToken);
                });

            var activityId = Guid.NewGuid().ToString("N");

            // Mock UpdateActivityAsync, assert the activity being sent
            _mockAdapter.Setup(x => x.DeleteActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<ConversationReference>(), It.IsAny<CancellationToken>()))
                .Callback<ITurnContext, ConversationReference, CancellationToken>((context, conversationReference, cancellationToken) =>
                {
                    // Assert the activityId being deleted.
                    Assert.Equal(activityId, conversationReference.ActivityId);
                });

            var sut = CreateSkillHandlerForTesting();
            await sut.TestOnDeleteActivityAsync(_claimsIdentity, _conversationId, activityId, CancellationToken.None);

            // Assert mock methods were called
            _mockAdapter.Verify(ma => ma.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<string>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            _mockAdapter.Verify(ma => ma.DeleteActivityAsync(It.IsAny<ITurnContext>(), It.IsAny<ConversationReference>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        [Fact]
        public async Task OnGetActivityMembersAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var activityId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                sut.TestOnGetActivityMembersAsync(_claimsIdentity, _conversationId, activityId, CancellationToken.None));
        }

        [Fact]
        public async Task OnCreateConversationAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationParameters = new ConversationParameters();
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                sut.TestOnCreateConversationAsync(_claimsIdentity, conversationParameters, CancellationToken.None));
        }

        [Fact]
        public async Task OnGetConversationsAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                sut.TestOnGetConversationsAsync(_claimsIdentity, conversationId, string.Empty, CancellationToken.None));
        }

        [Fact]
        public async Task OnGetConversationMembersAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                sut.TestOnGetConversationMembersAsync(_claimsIdentity, conversationId, CancellationToken.None));
        }

        [Fact]
        public async Task OnGetConversationPagedMembersAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                sut.TestOnGetConversationPagedMembersAsync(_claimsIdentity, conversationId, null, null, CancellationToken.None));
        }

        [Fact]
        public async Task OnDeleteConversationMemberAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            var memberId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                sut.TestOnDeleteConversationMemberAsync(_claimsIdentity, conversationId, memberId, CancellationToken.None));
        }

        [Fact]
        public async Task OnSendConversationHistoryAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            var transcript = new Transcript();
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                sut.TestOnSendConversationHistoryAsync(_claimsIdentity, conversationId, transcript, CancellationToken.None));
        }

        [Fact]
        public async Task OnUploadAttachmentAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            var attachmentData = new AttachmentData();
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                sut.TestOnUploadAttachmentAsync(_claimsIdentity, conversationId, attachmentData, CancellationToken.None));
        }

        private SkillHandlerInstanceForTests CreateSkillHandlerForTesting(SkillConversationIdFactoryBase overrideFactory = null)
        {
            return new SkillHandlerInstanceForTests(_mockAdapter.Object, _mockBot.Object, overrideFactory ?? _testConversationIdFactory, new Mock<ICredentialProvider>().Object, new AuthenticationConfiguration());
        }

        /// <summary>
        /// An in memory dictionary based ConversationIdFactory for testing.
        /// </summary>
        private class TestConversationIdFactory
            : SkillConversationIdFactoryBase
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
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// An in memory dictionary based ConversationIdFactory for testing.
        /// (Implements legacy/deprecated CreateSkillConversationIdAsync and GetConversationReferenceAsync).
        /// </summary>
        private class TestLegacyConversationIdFactory
            : SkillConversationIdFactoryBase
        {
            private readonly ConcurrentDictionary<string, string> _conversationRefs = new ConcurrentDictionary<string, string>();

            // Testing the deprecated method for backward compatibility.
#pragma warning disable 618
            [Obsolete]
            public override Task<string> CreateSkillConversationIdAsync(ConversationReference conversationReference, CancellationToken cancellationToken)
            {
#pragma warning restore 618
                var crJson = JsonConvert.SerializeObject(conversationReference);
                var key = (conversationReference.Conversation.Id + conversationReference.ServiceUrl).GetHashCode().ToString(CultureInfo.InvariantCulture);
                _conversationRefs.GetOrAdd(key, crJson);
                return Task.FromResult(key);
            }

            // Testing the deprecated method for backward compatibility.
#pragma warning disable 618
            [Obsolete]
            public override Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
#pragma warning restore 618
                var conversationReference = JsonConvert.DeserializeObject<ConversationReference>(_conversationRefs[skillConversationId]);
                return Task.FromResult(conversationReference);
            }

            public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// A helper class that provides public methods that allow us to test protected methods
        /// in <see cref="SkillHandler"/> bypassing authentication.
        /// </summary>
        private class SkillHandlerInstanceForTests : SkillHandler
        {
            public SkillHandlerInstanceForTests(BotAdapter adapter, IBot bot, SkillConversationIdFactoryBase testConversationIdFactory, ICredentialProvider credentialProvider, AuthenticationConfiguration authConfig, IChannelProvider channelProvider = null, ILogger logger = null)
                : base(adapter, bot, testConversationIdFactory, credentialProvider, authConfig, channelProvider, logger)
            {
            }

            public async Task<ResourceResponse> TestOnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default)
            {
                return await OnSendToConversationAsync(claimsIdentity, conversationId, activity, cancellationToken).ConfigureAwait(false);
            }

            public async Task<ResourceResponse> TestOnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
            {
                return await OnReplyToActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
            }

            public async Task<ResourceResponse> TestOnUpdateActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
            {
                return await OnUpdateActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
            }

            public async Task TestOnDeleteActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
            {
                await OnDeleteActivityAsync(claimsIdentity, conversationId, activityId, cancellationToken).ConfigureAwait(false);
            }

            public async Task<IList<ChannelAccount>> TestOnGetActivityMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
            {
                return await OnGetActivityMembersAsync(claimsIdentity, conversationId, activityId, cancellationToken).ConfigureAwait(false);
            }

            public async Task<ConversationResourceResponse> TestOnCreateConversationAsync(ClaimsIdentity claimsIdentity, ConversationParameters parameters, CancellationToken cancellationToken = default)
            {
                return await OnCreateConversationAsync(claimsIdentity, parameters, cancellationToken).ConfigureAwait(false);
            }

            public async Task<ConversationsResult> TestOnGetConversationsAsync(ClaimsIdentity claimsIdentity, string conversationId, string continuationToken = default, CancellationToken cancellationToken = default)
            {
                return await OnGetConversationsAsync(claimsIdentity, conversationId, continuationToken, cancellationToken).ConfigureAwait(false);
            }

            public async Task<IList<ChannelAccount>> TestOnGetConversationMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, CancellationToken cancellationToken = default)
            {
                return await OnGetConversationMembersAsync(claimsIdentity, conversationId, cancellationToken).ConfigureAwait(false);
            }

            public async Task<PagedMembersResult> TestOnGetConversationPagedMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, int? pageSize = default, string continuationToken = default, CancellationToken cancellationToken = default)
            {
                return await OnGetConversationPagedMembersAsync(claimsIdentity, conversationId, pageSize, continuationToken, cancellationToken).ConfigureAwait(false);
            }

            public async Task TestOnDeleteConversationMemberAsync(ClaimsIdentity claimsIdentity, string conversationId, string memberId, CancellationToken cancellationToken = default)
            {
                await OnDeleteConversationMemberAsync(claimsIdentity, conversationId, memberId, cancellationToken).ConfigureAwait(false);
            }

            public async Task<ResourceResponse> TestOnSendConversationHistoryAsync(ClaimsIdentity claimsIdentity, string conversationId, Transcript transcript, CancellationToken cancellationToken = default)
            {
                return await OnSendConversationHistoryAsync(claimsIdentity, conversationId, transcript, cancellationToken).ConfigureAwait(false);
            }

            public async Task<ResourceResponse> TestOnUploadAttachmentAsync(ClaimsIdentity claimsIdentity, string conversationId, AttachmentData attachmentUpload, CancellationToken cancellationToken = default)
            {
                return await OnUploadAttachmentAsync(claimsIdentity, conversationId, attachmentUpload, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
