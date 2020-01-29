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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests.Skills
{
    [TestClass]
    public class SkillHandlerTests
    {
        private readonly ClaimsIdentity _claimsIdentity;
        private readonly string _conversationId;
        private readonly ConversationReference _conversationReference;
        private readonly Mock<BotAdapter> _mockAdapter = new Mock<BotAdapter>();
        private readonly Mock<IBot> _mockBot = new Mock<IBot>();
        private readonly TestConversationIdFactory _testConversationIdFactory = new TestConversationIdFactory();

        public SkillHandlerTests()
        {
            var botId = Guid.NewGuid().ToString("N");
            var skillId = Guid.NewGuid().ToString("N");
            _claimsIdentity = new ClaimsIdentity();
            _claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, botId));
            _claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AppIdClaim, skillId));
            _claimsIdentity.AddClaim(new Claim(AuthenticationConstants.ServiceUrlClaim, "http://testbot.com/api/messages"));
            _conversationReference = new ConversationReference
            {
                Conversation = new ConversationAccount(id: Guid.NewGuid().ToString("N")),
                ServiceUrl = "http://testbot.com/api/messages"
            };
            _conversationId = _testConversationIdFactory.CreateSkillConversationIdAsync(_conversationReference, CancellationToken.None).Result;
        }

        [TestMethod]
        public async Task OnSendToConversationAsyncTest()
        {
            BotCallbackHandler botCallback = null;
            _mockAdapter.Setup(x => x.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, BotCallbackHandler, CancellationToken>((identity, reference, callback, cancellationToken) =>
                {
                    botCallback = callback;
                    Console.WriteLine("blah");
                });

            var sut = CreateSkillHandlerForTesting();

            var activity = Activity.CreateMessageActivity();
            activity.ApplyConversationReference(_conversationReference);

            await sut.TestOnSendToConversationAsync(_claimsIdentity, _conversationId, (Activity)activity, CancellationToken.None);
            Assert.IsNotNull(botCallback);
            await botCallback.Invoke(new TurnContext(_mockAdapter.Object, (Activity)activity), CancellationToken.None);
        }

        [TestMethod]
        public async Task OnOnReplyToActivityAsyncTest()
        {
            BotCallbackHandler botCallback = null;
            _mockAdapter.Setup(x => x.ContinueConversationAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<ConversationReference>(), It.IsAny<BotCallbackHandler>(), It.IsAny<CancellationToken>()))
                .Callback<ClaimsIdentity, ConversationReference, BotCallbackHandler, CancellationToken>((identity, reference, callback, cancellationToken) =>
                {
                    botCallback = callback;
                    Console.WriteLine("blah");
                });

            var sut = CreateSkillHandlerForTesting();

            var activity = Activity.CreateMessageActivity();
            var activityId = Guid.NewGuid().ToString("N");
            activity.ApplyConversationReference(_conversationReference);

            await sut.TestOnReplyToActivityAsync(_claimsIdentity, _conversationId, activityId, (Activity)activity, CancellationToken.None);
            Assert.IsNotNull(botCallback);
            await botCallback.Invoke(new TurnContext(_mockAdapter.Object, (Activity)activity), CancellationToken.None);
        }

        [TestMethod]
        public async Task OnUpdateActivityAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var activity = Activity.CreateMessageActivity();
            var activityId = Guid.NewGuid().ToString("N");
            activity.ApplyConversationReference(_conversationReference);

            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnUpdateActivityAsync(_claimsIdentity, _conversationId, activityId, (Activity)activity, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task OnDeleteActivityAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var activityId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnDeleteActivityAsync(_claimsIdentity, _conversationId, activityId, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task OnGetActivityMembersAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var activityId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnGetActivityMembersAsync(_claimsIdentity, _conversationId, activityId, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task OnCreateConversationAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationParameters = new ConversationParameters();
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnCreateConversationAsync(_claimsIdentity, conversationParameters, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task OnGetConversationsAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnGetConversationsAsync(_claimsIdentity, conversationId, string.Empty, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task OnGetConversationMembersAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnGetConversationMembersAsync(_claimsIdentity, conversationId, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task OnGetConversationPagedMembersAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnGetConversationPagedMembersAsync(_claimsIdentity, conversationId, null, null, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task OnDeleteConversationMemberAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            var memberId = Guid.NewGuid().ToString("N");
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnDeleteConversationMemberAsync(_claimsIdentity, conversationId, memberId, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task OnSendConversationHistoryAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            var transcript = new Transcript();
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnSendConversationHistoryAsync(_claimsIdentity, conversationId, transcript, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task OnUploadAttachmentAsyncTest()
        {
            var sut = CreateSkillHandlerForTesting();
            var conversationId = Guid.NewGuid().ToString("N");
            var attachmentData = new AttachmentData();
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sut.TestOnUploadAttachmentAsync(_claimsIdentity, conversationId, attachmentData, CancellationToken.None);
            });
        }

        private SkillHandlerInstanceForTests CreateSkillHandlerForTesting()
        {
            return new SkillHandlerInstanceForTests(_mockAdapter.Object, _mockBot.Object, _testConversationIdFactory, new Mock<ICredentialProvider>().Object, new AuthenticationConfiguration());
        }

        /// <summary>
        /// An in memory dictionary based ConversationIdFactory for testing.
        /// </summary>
        private class TestConversationIdFactory
            : SkillConversationIdFactoryBase
        {
            private readonly ConcurrentDictionary<string, string> _conversationRefs = new ConcurrentDictionary<string, string>();

            public override Task<string> CreateSkillConversationIdAsync(ConversationReference conversationReference, CancellationToken cancellationToken)
            {
                var crJson = JsonConvert.SerializeObject(conversationReference);
                var key = (conversationReference.Conversation.Id + conversationReference.ServiceUrl).GetHashCode().ToString(CultureInfo.InvariantCulture);
                _conversationRefs.GetOrAdd(key, crJson);
                return Task.FromResult(key);
            }

            public override Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                var conversationReference = JsonConvert.DeserializeObject<ConversationReference>(_conversationRefs[skillConversationId]);
                return Task.FromResult(conversationReference);
            }

            public override Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// A helper class that provides public methods that allow us to test protected methods in <see cref="SkillHandler"/> bypassing authentication.
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
