// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
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
        private ITurnContext _turnContext;

        [TestMethod]
        public async Task HandleActivitiesFromChannel()
        {
            var dialog = new SimpleComponentDialog();
            var testFlow = CreateFlow(dialog);
            await testFlow.Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send("SomeName")
                .AssertReply("Hello SomeName, nice to meet you!")
                .StartTestAsync();

            Assert.AreEqual(DialogReason.EndCalled, dialog.EndReason);
        }

        [TestMethod]
        public async Task SkillHandlesActivitiesFromParent()
        {
            var dialog = new SimpleComponentDialog();
            var testFlow = CreateFlow(dialog, true);
            await testFlow.Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send("SomeName")
                .AssertReply("Hello SomeName, nice to meet you!")
                .AssertReply(activity =>
                {
                    Assert.AreEqual(activity.Type, ActivityTypes.EndOfConversation);
                    Assert.AreEqual(((Activity)activity).Value, "SomeName");
                })
                .StartTestAsync();

            Assert.AreEqual(DialogReason.EndCalled, dialog.EndReason);
        }

        [TestMethod]
        public async Task SkillHandlesEocFromParent()
        {
            var dialog = new SimpleComponentDialog();
            var testFlow = CreateFlow(dialog, true);
            await testFlow.Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send(new Activity(ActivityTypes.EndOfConversation) { CallerId = _parentBotId })
                .StartTestAsync();

            Assert.AreEqual(DialogReason.CancelCalled, dialog.EndReason);
        }

        [TestMethod]
        public async Task SkillHandlesRepromptFromParent()
        {
            var dialog = new SimpleComponentDialog();
            var testFlow = CreateFlow(dialog, true);
            await testFlow.Send("Hi")
                .AssertReply("Hello, what is your name?")
                .Send(new Activity(ActivityTypes.Event) { CallerId = _parentBotId, Name = DialogEvents.RepromptDialog })
                .AssertReply("Hello, what is your name?")
                .StartTestAsync();

            Assert.AreEqual(DialogReason.BeginCalled, dialog.EndReason);
        }

        private TestFlow CreateFlow(Dialog dialog, bool isSkillFlow = false)
        {
            var conversationId = Guid.NewGuid().ToString();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation(conversationId));
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                // Capture turnContext to help with assertions.
                _turnContext = turnContext;

                if (isSkillFlow)
                {
                    // Create a skill ClaimsIdentity and put it in TurnState so SkillValidation.IsSkillClaim() returns true.
                    var claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.VersionClaim, "2.0"));
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, _skillBotId));
                    claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AuthorizedParty, _parentBotId));
                    turnContext.TurnState.Add(BotAdapter.BotIdentityKey, claimsIdentity);
                }

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

        // A simple dialog for testing.
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
