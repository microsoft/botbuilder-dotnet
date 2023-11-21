﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class DialogExtensionsTests
    {
        // An App ID for a parent bot.
        private readonly string _parentBotId = Guid.NewGuid().ToString();

        // An App ID for a skill bot.
        private readonly string _skillBotId = Guid.NewGuid().ToString();

        // Captures an EndOfConversation if it was sent to help with assertions.
        private Activity _eocSent;

        /// <summary>
        /// Enum to handle different test cases.
        /// </summary>
        public enum FlowTestCase
        {
            /// <summary>
            /// RunAsync is executing on a root bot with no skills (typical standalone bot).
            /// </summary>
            RootBotOnly,

            /// <summary>
            /// RunAsync is executing on a root bot handling replies from a skill.
            /// </summary>
            RootBotConsumingSkill,

            /// <summary>
            /// RunAsync is executing in a skill that is called from a root and calling another skill.
            /// </summary>
            MiddleSkill,

            /// <summary>
            /// RunAsync is executing in a skill that is called from a parent (a root or another skill) but doesn't call another skill.
            /// </summary>
            LeafSkill
        }

        [Theory]
        [InlineData(FlowTestCase.RootBotOnly, false)]
        [InlineData(FlowTestCase.RootBotConsumingSkill, false)]
        [InlineData(FlowTestCase.MiddleSkill, true)]
        [InlineData(FlowTestCase.LeafSkill, true)]
        public async Task HandlesBotAndSkillsTestCases(FlowTestCase testCase, bool shouldSendEoc)
        {
            var dialog = new SimpleComponentDialog();
            var testFlow = CreateTestFlow(dialog, testCase, locale: "en-GB");
            await testFlow.Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send("SomeName")
                .AssertReply("Hello SomeName, nice to meet you!")
                .StartTestAsync();

            Assert.Equal(DialogReason.EndCalled, dialog.EndReason);

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
        public async Task SkillHandlesEocFromParent()
        {
            var dialog = new SimpleComponentDialog();
            var testFlow = CreateTestFlow(dialog, FlowTestCase.LeafSkill);
            await testFlow.Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send(new Activity(ActivityTypes.EndOfConversation) { CallerId = _parentBotId })
                .StartTestAsync();

            Assert.Null(_eocSent);
            Assert.Equal(DialogReason.CancelCalled, dialog.EndReason);
        }

        [Fact]
        public async Task SkillHandlesRepromptFromParent()
        {
            var dialog = new SimpleComponentDialog();
            var testFlow = CreateTestFlow(dialog, FlowTestCase.LeafSkill);
            await testFlow.Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send(new Activity(ActivityTypes.Event)
                {
                    CallerId = _parentBotId,
                    Name = DialogEvents.RepromptDialog
                })
                .AssertReply("Hello, what is your name?")
                .StartTestAsync();

            Assert.Equal(DialogReason.BeginCalled, dialog.EndReason);
        }

        [Fact]
        public async Task RunAsyncShouldSetTelemetryClient()
        {
            var adapter = new Mock<BotAdapter>();
            var dialog = new SimpleComponentDialog();
            var conversationState = new ConversationState(new MemoryStorage());

            // ChannelId and Conversation.Id are required for ConversationState and
            // ChannelId and From.Id are required for UserState.
            var activity = new Activity
            {
                ChannelId = "test-channel",
                Conversation = new ConversationAccount { Id = "test-conversation-id" },
                From = new ChannelAccount { Id = "test-id" }
            };

            var telemetryClientMock = new Mock<IBotTelemetryClient>();

            using (var turnContext = new TurnContext(adapter.Object, activity))
            {
                turnContext.TurnState.Add(telemetryClientMock.Object);

                await DialogExtensions.RunAsync(dialog, turnContext, conversationState.CreateProperty<DialogState>("DialogState"), CancellationToken.None);
            }

            Assert.Equal(telemetryClientMock.Object, dialog.TelemetryClient);
        }

        [Fact]
        public async Task RunAsyncWithCircularReferenceExceptionShouldFail()
        {
            var conversationReference = TestAdapter.CreateConversation(nameof(RunAsyncWithCircularReferenceExceptionShouldFail));

            var adapter = new TestAdapter(conversationReference)
                   .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new MemoryStorage()));

            try
            {
                var dm = new DialogManager(new AdaptiveDialog("adaptiveDialog")
                {
                    Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new CustomExceptionDialog()
                        }
                    }
                }
                });

                await new TestFlow((TestAdapter)adapter, dm.OnTurnAsync)
                    .Send("hi")
                    .StartTestAsync();
            }
            catch (AggregateException ex)
            {
                Assert.Equal(2, ex.InnerExceptions.Count);
            }
        }

        /// <summary>
        /// Creates a TestFlow instance with state data to recreate and assert the different test case.
        /// </summary>
        private TestFlow CreateTestFlow(Dialog dialog, FlowTestCase testCase, string locale = null)
        {
            var conversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(conversationId));
            adapter
                .UseStorage(storage)
                .UseBotState(userState, convoState)
                .Use(new AutoSaveStateMiddleware(userState, convoState))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            if (!string.IsNullOrEmpty(locale))
            {
                adapter.Locale = locale;
            }

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                if (testCase != FlowTestCase.RootBotOnly)
                {
                    // Create a skill ClaimsIdentity and put it in TurnState so SkillValidation.IsSkillClaim() returns true.
                    var claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, _skillBotId));
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AuthorizedParty, _parentBotId));
                    turnContext.TurnState.Add(BotAdapter.BotIdentityKey, claimsIdentity);

                    if (testCase == FlowTestCase.RootBotConsumingSkill)
                    {
                        // Simulate the SkillConversationReference with a channel OAuthScope stored in TurnState.
                        // This emulates a response coming to a root bot through SkillHandler. 
                        turnContext.TurnState.Add(SkillHandler.SkillConversationReferenceKey, new SkillConversationReference { OAuthScope = AuthenticationConstants.ToChannelFromBotOAuthScope });
                    }

                    if (testCase == FlowTestCase.MiddleSkill)
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

                // Invoke RunAsync on the dialog.
                await dialog.RunAsync(turnContext, convoState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            });
        }

        // A simple two step waterfall component dialog for testing.
        private class SimpleComponentDialog : ComponentDialog
        {
            public SimpleComponentDialog()
                : base(nameof(SimpleComponentDialog))
            {
                AddDialog(new TextPrompt(nameof(TextPrompt)));
                var waterfallSteps = new WaterfallStep[]
                {
                    PromptForNameAsync,
                    FinalStepAsync
                };
                AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

                InitialDialogId = nameof(WaterfallDialog);
            }

            /// <summary>
            /// Gets the <see cref="DialogReason"/> for the dialog termination to help with assertions.
            /// </summary>
            /// <remarks>
            /// RunAsync doesn't return dialog turn results so we need to use this to assert how the dialog ended.
            /// </remarks>
            public DialogReason EndReason { get; private set; }

            protected override Task OnEndDialogAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default)
            {
                // Capture the end reason for assertions. 
                EndReason = reason;
                return base.OnEndDialogAsync(context, instance, reason, cancellationToken);
            }

            private static async Task<DialogTurnResult> PromptForNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Hello, what is your name?", InputHints.ExpectingInput),
                        RetryPrompt = MessageFactory.Text("Hello, what is your name again?", InputHints.ExpectingInput)
                    },
                    cancellationToken);
            }

            private static async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                await stepContext.Context.SendActivityAsync($"Hello {stepContext.Result}, nice to meet you!", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(stepContext.Result, cancellationToken);
            }
        }

        private class CustomExceptionDialog : Dialog
        {
            public CustomExceptionDialog()
                : base("custom-exception")
            {
            }

            public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
            {
                var e1 = new CustomException("Parent: Self referencing Exception");
                var e2 = new CustomException("Child: Self referencing Exception", e1);
                e1.Children.Add(e2);
                throw e1;
            }

            private class CustomException : Exception
            {
#pragma warning disable SA1401 // Fields should be private
                public List<CustomException> Children = new List<CustomException>();
                public CustomException Parent;
#pragma warning restore SA1401 // Fields should be private

                public CustomException(string message, CustomException parent = null)
                    : base("Error: " + message)
                {
                    Parent = parent;
                }
            }
        }
    }
}
