// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
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

        [TestMethod]
        [DataRow(FlowTestCase.RootBotOnly, false)]
        [DataRow(FlowTestCase.RootBotConsumingSkill, false)]
        [DataRow(FlowTestCase.MiddleSkill, true)]
        [DataRow(FlowTestCase.LeafSkill, true)]
        public async Task HandlesBotAndSkillsTestCases(FlowTestCase testCase, bool shouldSendEoc)
        {
            var dialog = new SimpleComponentDialog();
            var testFlow = CreateTestFlow(dialog, testCase);
            await testFlow.Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send("SomeName")
                .AssertReply("Hello SomeName, nice to meet you!")
                .StartTestAsync();
            
            Assert.AreEqual(DialogReason.EndCalled, dialog.EndReason);

            if (shouldSendEoc)
            {
                Assert.IsNotNull(_eocSent, "Skills should send EndConversation to channel");
                Assert.AreEqual(ActivityTypes.EndOfConversation, _eocSent.Type);
                Assert.AreEqual("SomeName", _eocSent.Value);
            }
            else
            {
                Assert.IsNull(_eocSent, "Root bot should not send EndConversation to channel");
            }
        }

        [TestMethod]
        public async Task SkillHandlesEocFromParent()
        {
            var dialog = new SimpleComponentDialog();
            var testFlow = CreateTestFlow(dialog, FlowTestCase.LeafSkill);
            await testFlow.Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send(new Activity(ActivityTypes.EndOfConversation) { CallerId = _parentBotId })
                .StartTestAsync();

            Assert.IsNull(_eocSent, "Skill should not send back EoC when an EoC is sent from a parent");
            Assert.AreEqual(DialogReason.CancelCalled, dialog.EndReason);
        }

        [TestMethod]
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

            Assert.AreEqual(DialogReason.BeginCalled, dialog.EndReason);
        }

        /// <summary>
        /// Creates a TestFlow instance with state data to recreate and assert the different test case.
        /// </summary>
        private TestFlow CreateTestFlow(Dialog dialog, FlowTestCase testCase)
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
    }
}
