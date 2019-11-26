// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Bot.Builder.Skills.Tests
{
    // TODO: need to simplify this tests
    public class SkillHandlerTests
    {
        [Fact(Skip = "TODO: I moved this logic to ChannelServiceHandler.InvokeChannelApiAsync, rewrite this test once i finish with refactoring. Gabo")]
        public void TestSkillAdapterInjectsMiddleware()
        {
            var botAdapter = CreateAdapter("TestSkillAdapterInjectsMiddleware");

            var skillClient = new SkillHandler(botAdapter, new CallbackBot(), new TestConversationIdFactory(),  new Mock<ICredentialProvider>().Object, new AuthenticationConfiguration());

            Assert.Equal(1, botAdapter.MiddlewareSet.Count(s => s is ChannelApiMiddleware));
        }

        [Fact]
        public async Task TestSkillAdapterApiCalls()
        {
            var activityId = Guid.NewGuid().ToString("N");
            var botId = Guid.NewGuid().ToString("N");
            var botAdapter = CreateAdapter("TestSkillAdapterApiCalls");
            var skillAccount = ObjectPath.Clone(botAdapter.Conversation.Bot);
            var skillId = "testSkill";
            skillAccount.Properties["SkillId"] = skillId;

            var middleware = new AssertInvokeMiddleware(botAdapter, activityId);
            botAdapter.Use(middleware);
            var bot = new CallbackBot();
            var skillClient = new SkillHandlerInstanceForTests(botAdapter, bot, new Mock<ICredentialProvider>().Object, new AuthenticationConfiguration());

            var sc = new TestConversationIdFactory();

            var skillConversationId = sc.CreateSkillConversationId(botAdapter.Conversation.Conversation.Id, botAdapter.Conversation.ServiceUrl);
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, botId));
            claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AppIdClaim, botId));
            claimsIdentity.AddClaim(new Claim(AuthenticationConstants.ServiceUrlClaim, botAdapter.Conversation.ServiceUrl));

            object result = await skillClient.TestOnCreateConversationAsync(claimsIdentity, new ConversationParameters());
            Assert.IsType<ConversationResourceResponse>(result);
            
            //Assert.Equal(middleware.NewResourceId, ((ConversationResourceResponse)result).Id);

            await skillClient.TestOnDeleteActivityAsync(claimsIdentity, skillConversationId, activityId);

            await skillClient.TestOnDeleteConversationMemberAsync(claimsIdentity, skillConversationId, "user2");

            result = await skillClient.TestOnGetActivityMembersAsync(claimsIdentity, skillConversationId, activityId);
            Assert.IsAssignableFrom<IList<ChannelAccount>>(result);

            result = await skillClient.TestOnGetConversationMembersAsync(claimsIdentity, skillConversationId);
            Assert.IsAssignableFrom<IList<ChannelAccount>>(result);

            result = await skillClient.TestOnGetConversationPagedMembersAsync(claimsIdentity, skillConversationId);
            Assert.IsType<PagedMembersResult>(result);

            result = await skillClient.TestOnGetConversationPagedMembersAsync(claimsIdentity, skillConversationId, 10);
            Assert.IsType<PagedMembersResult>(result);

            var pagedMembersResult = (PagedMembersResult)result;
            result = await skillClient.TestOnGetConversationPagedMembersAsync(claimsIdentity, skillConversationId, continuationToken: pagedMembersResult.ContinuationToken);
            Assert.IsType<PagedMembersResult>(result);

            result = await skillClient.TestOnGetConversationsAsync(claimsIdentity, skillConversationId);
            Assert.IsType<ConversationsResult>(result);

            var conversationsResult = (ConversationsResult)result;
            result = await skillClient.TestOnGetConversationsAsync(claimsIdentity, skillConversationId, continuationToken: conversationsResult.ContinuationToken);
            Assert.IsType<ConversationsResult>(result);

            var msgActivity = Activity.CreateMessageActivity();
            msgActivity.Conversation = botAdapter.Conversation.Conversation;
            msgActivity.Text = "yo";

            result = await skillClient.TestOnSendToConversationAsync(claimsIdentity, skillConversationId, (Activity)msgActivity);
            Assert.IsType<ResourceResponse>(result);
            Assert.Equal(middleware.NewResourceId, ((ResourceResponse)result).Id);
            msgActivity.Id = ((ResourceResponse)result).Id;

            result = await skillClient.TestOnReplyToActivityAsync(claimsIdentity, skillConversationId, activityId, (Activity)msgActivity);
            Assert.IsType<ResourceResponse>(result);
            Assert.Equal(middleware.NewResourceId, ((ResourceResponse)result).Id);
            msgActivity.Id = ((ResourceResponse)result).Id;

            result = await skillClient.TestOnSendConversationHistoryAsync(claimsIdentity, skillConversationId, new Transcript());
            Assert.IsType<ResourceResponse>(result);
            Assert.Equal(middleware.NewResourceId, ((ResourceResponse)result).Id);

            result = await skillClient.TestOnUpdateActivityAsync(claimsIdentity, skillConversationId, activityId, (Activity)msgActivity);
            Assert.IsType<ResourceResponse>(result);
            Assert.Equal(middleware.NewResourceId, ((ResourceResponse)result).Id);

            result = await skillClient.TestOnUploadAttachmentAsync(claimsIdentity, skillConversationId, new AttachmentData());
            Assert.IsType<ResourceResponse>(result);
            Assert.Equal(middleware.NewResourceId, ((ResourceResponse)result).Id);
        }

        private TestAdapter CreateAdapter(string conversationName)
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(conversationName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));
            return adapter;
        }

        private class CallbackBot : IBot
        {
            private readonly BotCallbackHandler _callback;

            public CallbackBot(BotCallbackHandler callback = null)
            {
                _callback = callback;
            }

            public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
            {
                return _callback != null ? _callback(turnContext, cancellationToken) : Task.CompletedTask;
            }
        }

        private class AssertInvokeMiddleware : IMiddleware
        {
            private readonly TestAdapter _adapter;
            private readonly string _expectedActivityId;

            public AssertInvokeMiddleware(TestAdapter adapter, string activityId)
            {
                _adapter = adapter;
                _expectedActivityId = activityId;
                NewResourceId = Guid.NewGuid().ToString("n");
            }

            public string NewResourceId { get; }

            public Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
            {
                Assert.Equal(ActivityTypes.Invoke, turnContext.Activity.Type);
                Assert.Equal(_adapter.Conversation.Conversation.Id, turnContext.Activity.Conversation.Id);
                Assert.Equal(_adapter.Conversation.ServiceUrl, turnContext.Activity.ServiceUrl);
                Assert.Equal(_adapter.Conversation.User.Id, turnContext.Activity.From.Id);
                Assert.Equal(_adapter.Conversation.Bot.Id, turnContext.Activity.Recipient.Id);

                var invoke = turnContext.Activity.AsInvokeActivity();
                Assert.Equal(SkillHandler.InvokeActivityName, invoke.Name);
                var apiArgs = invoke.Value as ChannelApiArgs;
                Assert.NotNull(apiArgs);

                switch (apiArgs.Method)
                {
                    // ReplyToActivity(conversationId, activityId, activity).
                    case ChannelApiMethods.ReplyToActivity:
                        Assert.Equal(2, apiArgs.Args.Length);
                        Assert.IsType<string>(apiArgs.Args[0]);
                        Assert.IsType<Activity>(apiArgs.Args[1]);
                        apiArgs.Result = new ResourceResponse(id: NewResourceId);
                        break;

                    // SendToConversation(activity).
                    case ChannelApiMethods.SendToConversation:
                        Assert.Single(apiArgs.Args);
                        Assert.IsType<Activity>(apiArgs.Args[0]);
                        apiArgs.Result = new ResourceResponse(id: NewResourceId);
                        break;

                    // UpdateActivity(activity).
                    case ChannelApiMethods.UpdateActivity:
                        Assert.Equal(2, apiArgs.Args.Length);
                        Assert.IsType<string>(apiArgs.Args[0]);
                        Assert.IsType<Activity>(apiArgs.Args[1]);
                        Assert.Equal(_expectedActivityId, (string)apiArgs.Args[0]);
                        apiArgs.Result = new ResourceResponse(id: NewResourceId);
                        break;

                    // DeleteActivity(conversationId, activityId).
                    case ChannelApiMethods.DeleteActivity:
                        Assert.Single(apiArgs.Args);
                        Assert.IsType<string>(apiArgs.Args[0]);
                        Assert.Equal(_expectedActivityId, apiArgs.Args[0]);
                        break;

                    // SendConversationHistory(conversationId, history).
                    case ChannelApiMethods.SendConversationHistory:
                        Assert.Single(apiArgs.Args);
                        Assert.IsType<Transcript>(apiArgs.Args[0]);
                        apiArgs.Result = new ResourceResponse(id: NewResourceId);
                        break;

                    // GetConversationMembers(conversationId).
                    case ChannelApiMethods.GetConversationMembers:
                        Assert.Empty(apiArgs.Args);
                        apiArgs.Result = new List<ChannelAccount>();
                        break;

                    // GetConversationPageMembers(conversationId, (int)pageSize, continuationToken).
                    case ChannelApiMethods.GetConversationPagedMembers:
                        Assert.Equal(2, apiArgs.Args.Length);

                        if (apiArgs.Args[0] != null)
                        {
                            Assert.Equal(10, ((int?)apiArgs.Args[0]).Value);
                        }

                        if (apiArgs.Args[1] != null)
                        {
                            Assert.Equal("continue please", (string)apiArgs.Args[1]);
                        }

                        apiArgs.Result = new PagedMembersResult
                        {
                            ContinuationToken = "continue please",
                            Members = new ChannelAccount[] { }
                        };
                        break;

                    // DeleteConversationMember(conversationId, memberId).
                    case ChannelApiMethods.DeleteConversationMember:
                        Assert.Single(apiArgs.Args);
                        Assert.IsType<string>(apiArgs.Args[0]);
                        break;

                    // GetActivityMembers(conversationId, activityId).
                    case ChannelApiMethods.GetActivityMembers:
                        Assert.Single(apiArgs.Args);
                        Assert.IsType<string>(apiArgs.Args[0]);
                        apiArgs.Result = new List<ChannelAccount>();
                        break;

                    // UploadAttachment(conversationId, attachmentData).
                    case ChannelApiMethods.UploadAttachment:
                        Assert.Single(apiArgs.Args);
                        Assert.IsType<AttachmentData>(apiArgs.Args[0]);
                        apiArgs.Result = new ResourceResponse(id: NewResourceId);
                        break;

                    // CreateConversation([FromBody] ConversationParameters parameters)
                    case ChannelApiMethods.CreateConversation:
                        Assert.Single(apiArgs.Args);
                        Assert.IsType<ConversationParameters>(apiArgs.Args[0]);
                        apiArgs.Result = new ConversationResourceResponse(id: NewResourceId);
                        break;

                    // GetConversations(string continuationToken = null)
                    case ChannelApiMethods.GetConversations:
                        Assert.True(apiArgs.Args.Length == 0 || apiArgs.Args.Length == 1);
                        if (apiArgs.Args.Length == 1)
                        {
                            if (apiArgs.Args[0] != null)
                            {
                                Assert.IsType<string>(apiArgs.Args[0]);
                                Assert.Equal("continue please", (string)apiArgs.Args[0]);
                            }
                        }

                        apiArgs.Result = new ConversationsResult { ContinuationToken = "continue please" };
                        break;

                    default:
                        throw new XunitException($"Unknown ChannelApi method {apiArgs.Method}");
                }

                // return next(cancellationToken);
                return Task.CompletedTask;
            }
        }

        private class TestConversationIdFactory
            : ISkillConversationIdFactory
        {
            public string CreateSkillConversationId(string conversationId, string serviceUrl)
            {
                var jsonString = JsonConvert.SerializeObject(new[]
                {
                    conversationId,
                    serviceUrl
                });

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
            }

            public (string, string) GetConversationInfo(string conversationId)
            {
                if (conversationId == null)
                {
                    return (null, null);
                }

                var jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(conversationId));
                var parts = JsonConvert.DeserializeObject<string[]>(jsonString);
                return (parts[0], parts[1]);
            }
        }

        /// <summary>
        /// A helper class that provides public methods that allow us to test protected methods in <see cref="SkillHandler"/>.
        /// </summary>
        private class SkillHandlerInstanceForTests : SkillHandler
        {
            public SkillHandlerInstanceForTests(BotAdapter adapter, IBot bot, ICredentialProvider credentialProvider, AuthenticationConfiguration authConfig, IChannelProvider channelProvider = null, ILogger logger = null)
                : base(adapter, bot, new TestConversationIdFactory(),  credentialProvider, authConfig, channelProvider, logger)
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
