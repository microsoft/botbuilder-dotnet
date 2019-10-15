#pragma warning disable SA1402 // File may only contain a single type
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Skills.Adapters;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Skills.Tests
{
    internal class CallbackBot : IBot
    {
        private BotCallbackHandler callback;

        public CallbackBot(BotCallbackHandler callback = null)
        {
            this.callback = callback;
        }

        public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            return (callback != null) ? callback(turnContext, cancellationToken) : Task.CompletedTask;
        }
    }

    internal class AssertInvokeMiddleware : IMiddleware
    {
        private TestAdapter adapter;
        private string skillId;
        private string expectedActivityId;

        public AssertInvokeMiddleware(TestAdapter adapter, string skillId, string activityId)
        {
            this.adapter = adapter;
            this.skillId = skillId;
            this.expectedActivityId = activityId;
            this.NewResourceId = Guid.NewGuid().ToString("n");
        }

        public string NewResourceId { get; set; }

        public Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            Assert.AreEqual(ActivityTypes.Invoke, turnContext.Activity.Type);
            Assert.AreEqual(adapter.Conversation.Conversation.Id, turnContext.Activity.Conversation.Id);
            Assert.AreEqual(adapter.Conversation.ServiceUrl, turnContext.Activity.ServiceUrl);
            Assert.AreEqual(adapter.Conversation.User.Id, turnContext.Activity.From.Id);
            Assert.AreEqual(adapter.Conversation.Bot.Id, turnContext.Activity.Recipient.Id);

            var invoke = turnContext.Activity.AsInvokeActivity();
            Assert.AreEqual("ChannelAPI", invoke.Name);
            var apiArgs = invoke.Value as ChannelApiArgs;
            Assert.IsNotNull(apiArgs);

            switch (apiArgs.Method)
            {
                /// <summary>
                /// ReplyToActivity(conversationId, activityId, activity).
                /// </summary>
                case ChannelApiMethods.ReplyToActivity:
                    Assert.AreEqual(2, apiArgs.Args.Length);
                    Assert.IsInstanceOfType(apiArgs.Args[0], typeof(string));
                    Assert.IsInstanceOfType(apiArgs.Args[1], typeof(Activity));
                    apiArgs.Result = new ResourceResponse(id: NewResourceId);
                    break;

                /// <summary>
                /// SendToConversation(activity).
                /// </summary>
                case ChannelApiMethods.SendToConversation:
                    Assert.AreEqual(1, apiArgs.Args.Length);
                    Assert.IsInstanceOfType(apiArgs.Args[0], typeof(Activity));
                    apiArgs.Result = new ResourceResponse(id: NewResourceId);
                    break;

                /// <summary>
                /// UpdateActivity(activity).
                /// </summary>
                case ChannelApiMethods.UpdateActivity:
                    Assert.AreEqual(2, apiArgs.Args.Length);
                    Assert.IsInstanceOfType(apiArgs.Args[0], typeof(string));
                    Assert.IsInstanceOfType(apiArgs.Args[1], typeof(Activity));
                    Assert.AreEqual(this.expectedActivityId, (string)apiArgs.Args[0]);
                    apiArgs.Result = new ResourceResponse(id: NewResourceId);
                    break;

                /// <summary>
                /// DeleteActivity(conversationId, activityId).
                /// </summary>
                case ChannelApiMethods.DeleteActivity:
                    Assert.AreEqual(1, apiArgs.Args.Length);
                    Assert.IsInstanceOfType(apiArgs.Args[0], typeof(string));
                    Assert.AreEqual(this.expectedActivityId, apiArgs.Args[0]);
                    break;

                /// <summary>
                /// SendConversationHistory(conversationId, history).
                /// </summary>
                case ChannelApiMethods.SendConversationHistory:
                    Assert.AreEqual(1, apiArgs.Args.Length);
                    Assert.IsInstanceOfType(apiArgs.Args[0], typeof(Transcript));
                    apiArgs.Result = new ResourceResponse(id: NewResourceId);
                    break;

                /// <summary>
                /// GetConversationMembers(conversationId).
                /// </summary>
                case ChannelApiMethods.GetConversationMembers:
                    Assert.AreEqual(0, apiArgs.Args.Length);
                    apiArgs.Result = new List<ChannelAccount>();
                    break;

                /// <summary>
                /// GetConversationPageMembers(conversationId, (int)pageSize, continuationToken).
                /// </summary>
                case ChannelApiMethods.GetConversationPagedMembers:
                    Assert.AreEqual(2, apiArgs.Args.Length);

                    if (apiArgs.Args[0] != null)
                    {
                        Assert.AreEqual(10, ((int?)apiArgs.Args[0]).Value);
                    }

                    if (apiArgs.Args[1] != null)
                    {
                        Assert.AreEqual("continue please", (string)apiArgs.Args[1]);
                    }

                    apiArgs.Result = new PagedMembersResult() { ContinuationToken = "continue please", Members = new ChannelAccount[] { } };
                    break;

                /// <summary>
                /// DeleteConversationMember(conversationId, memberId).
                /// </summary>
                case ChannelApiMethods.DeleteConversationMember:
                    Assert.AreEqual(1, apiArgs.Args.Length);
                    Assert.IsInstanceOfType(apiArgs.Args[0], typeof(string));
                    break;

                /// <summary>
                /// GetActivityMembers(conversationId, activityId).
                /// </summary>
                case ChannelApiMethods.GetActivityMembers:
                    Assert.AreEqual(1, apiArgs.Args.Length);
                    Assert.IsInstanceOfType(apiArgs.Args[0], typeof(string));
                    apiArgs.Result = new List<ChannelAccount>();
                    break;

                /// <summary>
                /// UploadAttachment(conversationId, attachmentData).
                /// </summary>
                case ChannelApiMethods.UploadAttachment:
                    Assert.AreEqual(1, apiArgs.Args.Length);
                    Assert.IsInstanceOfType(apiArgs.Args[0], typeof(AttachmentData));
                    apiArgs.Result = new ResourceResponse(id: NewResourceId);
                    break;

                /// <summary>
                /// CreateConversation([FromBody] ConversationParameters parameters)
                /// </summary>
                case ChannelApiMethods.CreateConversation:
                    Assert.AreEqual(1, apiArgs.Args.Length);
                    Assert.IsInstanceOfType(apiArgs.Args[0], typeof(ConversationParameters));
                    apiArgs.Result = new ConversationResourceResponse(id: NewResourceId);
                    break;

                /// <summary>
                /// GetConversations(string continuationToken = null)
                /// </summary>
                case ChannelApiMethods.GetConversations:
                    Assert.IsTrue(apiArgs.Args.Length == 0 || apiArgs.Args.Length == 1);
                    if (apiArgs.Args.Length == 1)
                    {
                        if (apiArgs.Args[0] != null)
                        {
                            Assert.IsInstanceOfType(apiArgs.Args[0], typeof(string));
                            Assert.AreEqual("continue please", (string)apiArgs.Args[0]);
                        }
                    }

                    apiArgs.Result = new ConversationsResult() { ContinuationToken = "continue please" };
                    break;

                default:
                    Assert.Fail($"Unknown ChannelApi method {apiArgs.Method}");
                    break;
            }

            // return next(cancellationToken);
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class SkillAdapterTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestSkillAdapterInjectsMiddleware()
        {
            var botAdapter = CreateAdapter();
            
            var skillAdapter = new BotFrameworkSkillAdapter(botAdapter, new CallbackBot(), new MicrosoftAppCredentials(string.Empty, string.Empty), configuration: new ConfigurationBuilder().Build());

            Assert.AreEqual(1, botAdapter.MiddlewareSet.Where(s => s is ChannelApiMiddleware).Count(), "Should have injected ChannelApiMiddleware into adapter");
        }

        [TestMethod]
        public async Task TestSkillAdapterAPICalls()
        {
            var activityId = Guid.NewGuid().ToString("N");
            var botId = Guid.NewGuid().ToString("N");
            var botAdapter = CreateAdapter();
            var skillAccount = ObjectPath.Clone(botAdapter.Conversation.Bot);
            string skillId = "testSkill";
            skillAccount.Properties["SkillId"] = skillId;

            var middleware = new AssertInvokeMiddleware(botAdapter, skillId, activityId);
            botAdapter.Use(middleware);
            var skillAdapter = new BotFrameworkSkillAdapter(botAdapter, new CallbackBot(), new MicrosoftAppCredentials(string.Empty, string.Empty), configuration: new ConfigurationBuilder().Build());

            SkillConversation sc = new SkillConversation() { ServiceUrl = botAdapter.Conversation.ServiceUrl, ConversationId = botAdapter.Conversation.Conversation.Id };
            var skillConversationId = sc.GetSkillConversationId();
            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, botId));
            claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AppIdClaim, botId));
            claimsIdentity.AddClaim(new Claim(AuthenticationConstants.ServiceUrlClaim, botAdapter.Conversation.ServiceUrl));

            object result;
            result = await skillAdapter.CreateConversationAsync(claimsIdentity, skillConversationId, new ConversationParameters());
            Assert.IsInstanceOfType(result, typeof(ConversationResourceResponse));
            Assert.AreEqual(middleware.NewResourceId, ((ConversationResourceResponse)result).Id);

            await skillAdapter.DeleteActivityAsync(claimsIdentity, skillConversationId, activityId);

            await skillAdapter.DeleteConversationMemberAsync(claimsIdentity, skillConversationId, "user2");

            result = await skillAdapter.GetActivityMembersAsync(claimsIdentity, skillConversationId, activityId);
            Assert.IsInstanceOfType(result, typeof(IList<ChannelAccount>));

            result = await skillAdapter.GetConversationMembersAsync(claimsIdentity, skillConversationId);
            Assert.IsInstanceOfType(result, typeof(IList<ChannelAccount>));

            result = await skillAdapter.GetConversationPagedMembersAsync(claimsIdentity, skillConversationId);
            Assert.IsInstanceOfType(result, typeof(PagedMembersResult));

            result = await skillAdapter.GetConversationPagedMembersAsync(claimsIdentity, skillConversationId, 10);
            Assert.IsInstanceOfType(result, typeof(PagedMembersResult));

            var pagedMembersResult = (PagedMembersResult)result;
            result = await skillAdapter.GetConversationPagedMembersAsync(claimsIdentity, skillConversationId, continuationToken: pagedMembersResult.ContinuationToken);
            Assert.IsInstanceOfType(result, typeof(PagedMembersResult));

            result = await skillAdapter.GetConversationsAsync(claimsIdentity, skillConversationId);
            Assert.IsInstanceOfType(result, typeof(ConversationsResult));

            var conversationsResult = (ConversationsResult)result;
            result = await skillAdapter.GetConversationsAsync(claimsIdentity, skillConversationId, continuationToken: conversationsResult.ContinuationToken);
            Assert.IsInstanceOfType(result, typeof(ConversationsResult));

            var msgActivity = Activity.CreateMessageActivity();
            msgActivity.Conversation = botAdapter.Conversation.Conversation;
            msgActivity.From = skillAccount;
            msgActivity.Recipient = botAdapter.Conversation.User;
            msgActivity.Text = "yo";

            result = await skillAdapter.SendToConversationAsync(claimsIdentity, skillConversationId, (Activity)msgActivity);
            Assert.IsInstanceOfType(result, typeof(ResourceResponse));
            Assert.AreEqual(middleware.NewResourceId, ((ResourceResponse)result).Id);
            msgActivity.Id = ((ResourceResponse)result).Id;

            result = await skillAdapter.ReplyToActivityAsync(claimsIdentity, skillConversationId, activityId, (Activity)msgActivity);
            Assert.IsInstanceOfType(result, typeof(ResourceResponse));
            Assert.AreEqual(middleware.NewResourceId, ((ResourceResponse)result).Id);
            result = await skillAdapter.SendConversationHistoryAsync(claimsIdentity, skillConversationId, new Transcript());
            Assert.IsInstanceOfType(result, typeof(ResourceResponse));
            Assert.AreEqual(middleware.NewResourceId, ((ResourceResponse)result).Id);

            result = await skillAdapter.UpdateActivityAsync(claimsIdentity, skillConversationId, activityId, (Activity)msgActivity);
            Assert.IsInstanceOfType(result, typeof(ResourceResponse));
            Assert.AreEqual(middleware.NewResourceId, ((ResourceResponse)result).Id);

            result = await skillAdapter.UploadAttachmentAsync(claimsIdentity, skillConversationId, new AttachmentData());
            Assert.IsInstanceOfType(result, typeof(ResourceResponse));
            Assert.AreEqual(middleware.NewResourceId, ((ResourceResponse)result).Id);
        }

        private TestAdapter CreateAdapter()
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));
            return adapter;
        }
    }
}
