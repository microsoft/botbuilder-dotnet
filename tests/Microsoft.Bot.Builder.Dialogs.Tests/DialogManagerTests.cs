// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class DialogManagerTests
    {
        // An App ID for a parent bot.
        private readonly string _parentBotId = Guid.NewGuid().ToString();

        // An App ID for a skill bot.
        private readonly string _skillBotId = Guid.NewGuid().ToString();

        // Property to capture the DialogManager turn results and do assertions.
        private DialogManagerResult _dmTurnResult;

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task DialogManager_ConversationState_PersistedAcrossTurns()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            await CreateFlow(adaptiveDialog, storage, firstConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogManager_AlternateProperty()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog("conversation.name");

            await CreateFlow(adaptiveDialog, storage, firstConversationId, dialogStateProperty: "dialogState")
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogManager_ConversationState_ClearedAcrossConversations()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var secondConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog("conversation.name");

            await CreateFlow(adaptiveDialog, storage, firstConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();

            await CreateFlow(adaptiveDialog, storage, secondConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("John")
                .AssertReply("Hello John, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogManager_UserState_PersistedAcrossConversations()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var secondConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog("user.name");

            await CreateFlow(adaptiveDialog, storage, firstConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();

            await CreateFlow(adaptiveDialog, storage, secondConversationId)
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogManager_UserState_NestedDialogs_PersistedAcrossConversations()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var secondConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var outerAdaptiveDialog = CreateTestDialog("user.name");

            var componentDialog = new ComponentDialog();
            componentDialog.AddDialog(outerAdaptiveDialog);

            await CreateFlow(componentDialog, storage, firstConversationId)
            .Send("hi")
                .AssertReply("Hello, what is your name?")
            .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();

            await CreateFlow(componentDialog, storage, secondConversationId)
            .Send("hi")
                .AssertReply("Hello Carlos, nice to meet you!")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DialogManager_OnErrorEvent_Leaf()
        {
            await TestUtilities.RunTestScript();
        }

        [TestMethod]
        public async Task DialogManager_OnErrorEvent_Parent()
        {
            await TestUtilities.RunTestScript();
        }

        [TestMethod]
        public async Task DialogManager_OnErrorEvent_Root()
        {
            await TestUtilities.RunTestScript();
        }

        [TestMethod]
        public async Task DialogManager_DialogSet()
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter();
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var rootDialog = new AdaptiveDialog()
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "conversation.dialogId",
                                Value = "test"
                            },
                            new BeginDialog()
                            {
                                Dialog = "=conversation.dialogId"
                            },
                            new BeginDialog()
                            {
                                Dialog = "test"
                            }
                        }
                    }
                }
            };

            var dm = new DialogManager(rootDialog);
            dm.Dialogs.Add(new SimpleDialog() { Id = "test" });

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
                })
                .SendConversationUpdate()
                    .AssertReply("simple")
                    .AssertReply("simple")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task SkillSendsEoCAndValuesAtDialogEnd()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            await CreateFlow(adaptiveDialog, storage, firstConversationId, isSkillFlow: true)
                .Send("hi")
                .AssertReply("Hello, what is your name?")
                .Send("Carlos")
                .AssertReply("Hello Carlos, nice to meet you!")
                .AssertReply(activity =>
                {
                    Assert.AreEqual(activity.Type, ActivityTypes.EndOfConversation);
                    Assert.AreEqual(((Activity)activity).Value, "Carlos");
                })
                .StartTestAsync();
            Assert.AreEqual(DialogTurnStatus.Complete, _dmTurnResult.TurnResult.Status);
        }

        [TestMethod]
        public async Task SkillHandlesEoCFromParent()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            var eocActivity = new Activity(ActivityTypes.EndOfConversation);

            await CreateFlow(adaptiveDialog, storage, firstConversationId, isSkillFlow: true, isSkillResponse: false)
                .Send("hi")
                .AssertReply("Hello, what is your name?")
                .Send(eocActivity)
                .StartTestAsync();

            Assert.AreEqual(DialogTurnStatus.Cancelled, _dmTurnResult.TurnResult.Status);
        }

        [TestMethod]
        public async Task SkillHandlesRepromptFromParent()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            var repromptEvent = new Activity(ActivityTypes.Event) { Name = DialogEvents.RepromptDialog };

            await CreateFlow(adaptiveDialog, storage, firstConversationId, isSkillFlow: true)
                .Send("hi")
                .AssertReply("Hello, what is your name?")
                .Send(repromptEvent)
                .AssertReply("Hello, what is your name?")
                .StartTestAsync();

            Assert.AreEqual(DialogTurnStatus.Waiting, _dmTurnResult.TurnResult.Status);
        }

        [TestMethod]
        public async Task SkillShouldReturnEmptyOnRepromptWithNoDialog()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            var repromptEvent = new Activity(ActivityTypes.Event) { Name = DialogEvents.RepromptDialog };

            await CreateFlow(adaptiveDialog, storage, firstConversationId, isSkillFlow: true)
                .Send(repromptEvent)
                .StartTestAsync();

            Assert.AreEqual(DialogTurnStatus.Empty, _dmTurnResult.TurnResult.Status);
        }

        private Dialog CreateTestDialog(string property)
        {
            return new AskForNameDialog(property.Replace(".", string.Empty), property);
        }

        private TestFlow CreateFlow(Dialog dialog, IStorage storage, string conversationId, string dialogStateProperty = null, bool isSkillFlow = false, bool isSkillResponse = true)
        {
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(conversationId));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var dm = new DialogManager(dialog, dialogStateProperty: dialogStateProperty);
            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                if (isSkillFlow)
                {
                    // Create a skill ClaimsIdentity and put it in TurnState so SkillValidation.IsSkillClaim() returns true.
                    var claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, _skillBotId));
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AuthorizedParty, _parentBotId));
                    turnContext.TurnState.Add(BotAdapter.BotIdentityKey, claimsIdentity);

                    if (isSkillResponse)
                    {
                        // Simulate the SkillConversationReference with a parent Bot ID stored in TurnState.
                        // This emulates a response coming to a skill from another skill through SkillHandler. 
                        turnContext.TurnState.Add(SkillHandler.SkillConversationReferenceKey, new SkillConversationReference { OAuthScope = _parentBotId });
                    }
                }

                // Capture the last DialogManager turn result for assertions.
                _dmTurnResult = await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }

        private class AskForNameDialog : ComponentDialog, IDialogDependencies
        {
            private readonly string _property;

            public AskForNameDialog(string id, string property)
                : base(id)
            {
                AddDialog(new TextPrompt("prompt"));
                _property = property;
            }

            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default)
            {
                if (outerDc.State.TryGetValue<string>(_property, out var result))
                {
                    await outerDc.Context.SendActivityAsync($"Hello {result}, nice to meet you!", cancellationToken: cancellationToken);
                    return await outerDc.EndDialogAsync(result, cancellationToken);
                }

                return await outerDc.BeginDialogAsync(
                        "prompt",
                        new PromptOptions
                        {
                            Prompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = "Hello, what is your name?"
                            },
                            RetryPrompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = "Hello, what is your name?"
                            }
                        },
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            public IEnumerable<Dialog> GetDependencies()
            {
                return Dialogs.GetDialogs();
            }

            public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
            {
                outerDc.State.SetValue(_property, result);
                await outerDc.Context.SendActivityAsync($"Hello {result}, nice to meet you!", cancellationToken: cancellationToken);
                return await outerDc.EndDialogAsync(result, cancellationToken);
            }
        }

        private class SimpleDialog : Dialog
        {
            public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                await dc.Context.SendActivityAsync("simple", cancellationToken: cancellationToken);
                return await dc.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }
    }
}
