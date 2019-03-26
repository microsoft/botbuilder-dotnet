// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class ReplaceDialogTest
    {
        [TestMethod]
        public async Task ReplaceDialogNoBranchAsync()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogStateAccessor = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter();
            var dialog = new FirstDialog();

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dialog.RunAsync(turnContext, dialogStateAccessor, cancellationToken);
                await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            })
            .Send("hello")
            .AssertReply("prompt one")
            .Send("hello")
            .AssertReply("prompt two")
            .Send("hello")
            .AssertReply("prompt three")
            .Send("hello")
            .AssertReply("*** WaterfallDialog End ***")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ReplaceDialogBranchAsync()
        {
            var conversationState = new ConversationState(new MemoryStorage());
            var dialogStateAccessor = conversationState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter();
            var dialog = new FirstDialog();

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dialog.RunAsync(turnContext, dialogStateAccessor, cancellationToken);
                await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            })
            .Send("hello")
            .AssertReply("prompt one")
            .Send("hello")
            .AssertReply("prompt two")
            .Send("replace")
            .AssertReply("*** WaterfallDialog End ***")
            .AssertReply("prompt four")
            .Send("hello")
            .AssertReply("prompt five")
            .Send("hello")
            .StartTestAsync();
        }

        private class FirstDialog : ComponentDialog
        {
            public FirstDialog()
                : base(nameof(FirstDialog))
            {
                var steps = new WaterfallStep[]
                {
                    StepOneAsync,
                    StepTwoAsync,
                    ReplaceStepAsync,
                    StepThreeAsync,
                    LastStepAsync
                };

                AddDialog(new TextPrompt(nameof(TextPrompt)));
                AddDialog(new SecondDialog());
                AddDialog(new WaterfallWithEndDialog(nameof(WaterfallWithEndDialog), steps));

                InitialDialogId = nameof(WaterfallWithEndDialog);
            }

            private static async Task<DialogTurnResult> StepOneAsync(WaterfallStepContext context, CancellationToken cancellationToken)
            {
                return await context.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("prompt one") });
            }

            private static async Task<DialogTurnResult> StepTwoAsync(WaterfallStepContext context, CancellationToken cancellationToken)
            {
                return await context.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("prompt two") });
            }

            private static async Task<DialogTurnResult> ReplaceStepAsync(WaterfallStepContext context, CancellationToken cancellationToken)
            {
                if (context.Result as string == "replace")
                {
                    return await context.ReplaceDialogAsync(nameof(SecondDialog));
                }
                else
                {
                    return await context.NextAsync();
                }
            }

            private static async Task<DialogTurnResult> StepThreeAsync(WaterfallStepContext context, CancellationToken cancellationToken)
            {
                return await context.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("prompt three") });
            }

            private static async Task<DialogTurnResult> LastStepAsync(WaterfallStepContext context, CancellationToken cancellationToken)
            {
                return await context.EndDialogAsync();
            }

            private class WaterfallWithEndDialog : WaterfallDialog
            {
                public WaterfallWithEndDialog(string id, WaterfallStep[] steps)
                    : base(id, steps)
                {
                }

                public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("*** WaterfallDialog End ***"));
                    await base.EndDialogAsync(turnContext, instance, reason, cancellationToken);
                }
            }
        }

        private class SecondDialog : ComponentDialog
        {
            public SecondDialog()
                : base(nameof(SecondDialog))
            {
                var steps = new WaterfallStep[]
                {
                    StepFourAsync,
                    StepFiveAsync,
                    LastStepAsync
                };

                AddDialog(new TextPrompt(nameof(TextPrompt)));
                AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));

                InitialDialogId = nameof(WaterfallDialog);
            }

            private static async Task<DialogTurnResult> StepFourAsync(WaterfallStepContext context, CancellationToken cancellationToken)
            {
                return await context.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("prompt four") });
            }

            private static async Task<DialogTurnResult> StepFiveAsync(WaterfallStepContext context, CancellationToken cancellationToken)
            {
                return await context.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("prompt five") });
            }

            private static async Task<DialogTurnResult> LastStepAsync(WaterfallStepContext context, CancellationToken cancellationToken)
            {
                return await context.EndDialogAsync();
            }
        }
    }
}
