// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Collections.Generic;
using System.Linq;
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
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class DialogManagerTests
    {
        // An App ID for a parent bot.
        private readonly string _parentBotId = Guid.NewGuid().ToString();

        // An App ID for a skill bot.
        private readonly string _skillBotId = Guid.NewGuid().ToString();

        // Captures an EndOfConversation if it was sent to help with assertions.
        private Activity _eocSent;

        // Property to capture the DialogManager turn results and do assertions.
        private DialogManagerResult _dmTurnResult;

        /// <summary>
        /// Enum to handle different skill test cases.
        /// </summary>
        public enum SkillFlowTestCase
        {
            /// <summary>
            /// DialogManager is executing on a root bot with no skills (typical standalone bot).
            /// </summary>
            RootBotOnly,

            /// <summary>
            /// DialogManager is executing on a root bot handling replies from a skill.
            /// </summary>
            RootBotConsumingSkill,

            /// <summary>
            /// DialogManager is executing in a skill that is called from a root and calling another skill.
            /// </summary>
            MiddleSkill,

            /// <summary>
            /// DialogManager is executing in a skill that is called from a parent (a root or another skill) but doesn't call another skill.
            /// </summary>
            LeafSkill
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public async Task DialogManager_OnErrorEvent_Leaf()
        {
            await TestUtilities.RunTestScript();
        }

        [Fact]
        public async Task DialogManager_OnErrorEvent_Parent()
        {
            await TestUtilities.RunTestScript();
        }

        [Fact]
        public async Task DialogManager_OnErrorEvent_Root()
        {
            await TestUtilities.RunTestScript();
        }

        [Fact]
        public async Task DialogManager_DialogSet()
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter();
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState)
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

        [Fact]
        public async Task DialogManager_ContainerRegistration()
        {
            var root = new AdaptiveDialog("root")
            {
                Triggers = new List<OnCondition>
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog> { new AdaptiveDialog("inner") }
                    }
                } 
            };

            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter();
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState);

            // The inner adaptive dialog should be registered on the DialogManager after OnTurn
            var dm = new DialogManager(root);
            
            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
                .SendConversationUpdate()
                .StartTestAsync();

            Assert.NotNull(dm.Dialogs.Find("inner"));
        }

        [Fact]
        public async Task DialogManager_ContainerRegistration_OnCyclicalDialogStructures()
        {
            var root = new AdaptiveDialog("root")
            {
                Triggers = new List<OnCondition>
                {
                    new OnBeginDialog()
                }
            };

            (root.Triggers.Single() as OnBeginDialog).Actions = new List<Dialog> { new EndTurn(), root };

            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter();
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState);

            // The inner adaptive dialog should be registered on the DialogManager after OnTurn.
            var dm = new DialogManager(root);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                // First OnTurn invocation will trigger registration of dependencies.
                // If registration is not protected against cyclical dialog structures, 
                // this call will throw StackOverflowException.
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
                .SendConversationUpdate()
                .StartTestAsync();
        }

        [Fact]
        public async Task DialogManager_ContainerRegistration_DoubleNesting()
        {
            // Create the following dialog tree
            // Root (adaptive) -> inner (adaptive) -> innerinner(adaptive) -> helloworld (SendActivity)
            var root = new AdaptiveDialog("root")
            {
                Triggers = new List<OnCondition>
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog> 
                        { 
                            new AdaptiveDialog("inner")
                            {
                                Triggers = new List<OnCondition>
                                {
                                    new OnBeginDialog()
                                    {
                                        Actions = new List<Dialog>
                                        {
                                            new AdaptiveDialog("innerinner")
                                            { 
                                                Triggers = new List<OnCondition>()
                                                { 
                                                    new OnBeginDialog()
                                                    { 
                                                        Actions = new List<Dialog>()
                                                        { 
                                                            new SendActivity("helloworld")
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter();
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState);

            // The inner adaptive dialog should be registered on the DialogManager after OnTurn
            var dm = new DialogManager(root);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            })
                .SendConversationUpdate()
                .StartTestAsync();

            // Top level containers should be registered
            Assert.NotNull(dm.Dialogs.Find("inner"));

            // Mid level containers should be registered
            Assert.NotNull(dm.Dialogs.Find("innerinner"));

            // Leaf nodes / non-contaners should not be registered
            Assert.DoesNotContain(dm.Dialogs.GetDialogs(), d => d.GetType() == typeof(SendActivity));
        }

        [Theory]
        [InlineData(SkillFlowTestCase.RootBotOnly, false)]
        [InlineData(SkillFlowTestCase.RootBotConsumingSkill, false)]
        [InlineData(SkillFlowTestCase.MiddleSkill, true)]
        [InlineData(SkillFlowTestCase.LeafSkill, true)]
        public async Task HandlesBotAndSkillsTestCases(SkillFlowTestCase testCase, bool shouldSendEoc)
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");
            await CreateFlow(adaptiveDialog, storage, firstConversationId, testCase: testCase, locale: "en-GB").Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send("SomeName")
                .AssertReply("Hello SomeName, nice to meet you!")
                .StartTestAsync();

            Assert.Equal(DialogTurnStatus.Complete, _dmTurnResult.TurnResult.Status);

            if (shouldSendEoc)
            {
                Assert.NotNull(_eocSent);
                Assert.Equal(ActivityTypes.EndOfConversation, _eocSent.Type);
                Assert.Equal(EndOfConversationCodes.CompletedSuccessfully, _eocSent.Code);
                Assert.Equal("SomeName", _eocSent.Value);
                Assert.Equal("en-GB", _eocSent.Locale);
            }
            else
            {
                Assert.Null(_eocSent);
            }
        }

        [Fact]
        public async Task SkillHandlesEoCFromParent()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            var eocActivity = new Activity(ActivityTypes.EndOfConversation);

            await CreateFlow(adaptiveDialog, storage, firstConversationId, testCase: SkillFlowTestCase.LeafSkill)
                .Send("hi")
                .AssertReply("Hello, what is your name?")
                .Send(eocActivity)
                .StartTestAsync();

            Assert.Equal(DialogTurnStatus.Cancelled, _dmTurnResult.TurnResult.Status);
        }

        [Fact]
        public async Task SkillHandlesRepromptFromParent()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            var repromptEvent = new Activity(ActivityTypes.Event) { Name = DialogEvents.RepromptDialog };

            await CreateFlow(adaptiveDialog, storage, firstConversationId, testCase: SkillFlowTestCase.LeafSkill)
                .Send("hi")
                .AssertReply("Hello, what is your name?")
                .Send(repromptEvent)
                .AssertReply("Hello, what is your name?")
                .StartTestAsync();

            Assert.Equal(DialogTurnStatus.Waiting, _dmTurnResult.TurnResult.Status);
        }

        [Fact]
        public async Task SkillShouldReturnEmptyOnRepromptWithNoDialog()
        {
            var firstConversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();

            var adaptiveDialog = CreateTestDialog(property: "conversation.name");

            var repromptEvent = new Activity(ActivityTypes.Event) { Name = DialogEvents.RepromptDialog };

            await CreateFlow(adaptiveDialog, storage, firstConversationId, testCase: SkillFlowTestCase.LeafSkill)
                .Send(repromptEvent)
                .StartTestAsync();

            Assert.Equal(DialogTurnStatus.Empty, _dmTurnResult.TurnResult.Status);
        }

        private Dialog CreateTestDialog(string property)
        {
            return new AskForNameDialog(property.Replace(".", string.Empty), property);
        }

        private TestFlow CreateFlow(Dialog dialog, IStorage storage, string conversationId, string dialogStateProperty = null, SkillFlowTestCase testCase = SkillFlowTestCase.RootBotOnly, string locale = null)
        {
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(conversationId));
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            if (!string.IsNullOrEmpty(locale))
            {
                adapter.Locale = locale;
            }

            var dm = new DialogManager(dialog, dialogStateProperty: dialogStateProperty);
            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                if (testCase != SkillFlowTestCase.RootBotOnly)
                {
                    // Create a skill ClaimsIdentity and put it in TurnState so SkillValidation.IsSkillClaim() returns true.
                    var claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, _skillBotId));
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AuthorizedParty, _parentBotId));
                    turnContext.TurnState.Add(BotAdapter.BotIdentityKey, claimsIdentity);

                    if (testCase == SkillFlowTestCase.RootBotConsumingSkill)
                    {
                        // Simulate the SkillConversationReference with a channel OAuthScope stored in TurnState.
                        // This emulates a response coming to a root bot through SkillHandler. 
                        turnContext.TurnState.Add(SkillHandler.SkillConversationReferenceKey, new SkillConversationReference { OAuthScope = AuthenticationConstants.ToChannelFromBotOAuthScope });
                    }

                    if (testCase == SkillFlowTestCase.MiddleSkill)
                    {
                        // Simulate the SkillConversationReference with a parent Bot ID stored in TurnState.
                        // This emulates a response coming to a skill from another skill through SkillHandler. 
                        turnContext.TurnState.Add(SkillHandler.SkillConversationReferenceKey, new SkillConversationReference { OAuthScope = _parentBotId });
                    }
                }

                // Interceptor to capture the EoC activity if it was sent so we can assert it in the tests.
                turnContext.OnSendActivities(async (tc, activities, next) =>
                {
                    _eocSent = activities.FirstOrDefault(activity => activity.Type == ActivityTypes.EndOfConversation);
                    return await next().ConfigureAwait(false);
                });

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
                        cancellationToken)
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
