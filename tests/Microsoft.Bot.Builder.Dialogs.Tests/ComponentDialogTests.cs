// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("ComponentDialog Tests")]
    public class ComponentDialogTests
    {
        [TestMethod]
        public async Task BasicWaterfallTest()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter();

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(CreateWaterfall());
                dialogs.Add(new NumberPrompt<int>("number", defaultLocale: Culture.English));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginAsync("test-waterfall", null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{value}'."), cancellationToken);
                }
                await convoState.SaveChangesAsync(turnContext);
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Thanks for '42'")
            .AssertReply("Enter another number.")
            .Send("64")
            .AssertReply("Bot received the number '64'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task BasicComponentDialogTest()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter();

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(new TestComponentDialog());

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginAsync("TestComponentDialog", null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{value}'."), cancellationToken);
                }
                await convoState.SaveChangesAsync(turnContext);
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Thanks for '42'")
            .AssertReply("Enter another number.")
            .Send("64")
            .AssertReply("Bot received the number '64'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NestedComponentDialogTest()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter();

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(new TestNestedComponentDialog());

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginAsync("TestNestedComponentDialog", null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{value}'."), cancellationToken);
                }
                await convoState.SaveChangesAsync(turnContext);
            })
            .Send("hello")

            // step 1
            .AssertReply("Enter a number.")

            // step 2
            .Send("42")
            .AssertReply("Thanks for '42'")
            .AssertReply("Enter another number.")

            // step 3 and step 1 again (nested component)
            .Send("64")
            .AssertReply("Got '64'.")
            .AssertReply("Enter a number.")

            // step 2 again (from the nested component)
            .Send("101")
            .AssertReply("Thanks for '101'")
            .AssertReply("Enter another number.")

            // driver code
            .Send("5")
            .AssertReply("Bot received the number '5'.")
            .StartTestAsync();
        }

        private static WaterfallDialog CreateWaterfall()
        {
            return new WaterfallDialog("test-waterfall", new WaterfallStep[] {
                WaterfallStep1,
                WaterfallStep2,
            });
        }

        private static async Task<DialogTurnResult> WaterfallStep1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter a number.") }, cancellationToken);
        }
        private static async Task<DialogTurnResult> WaterfallStep2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks for '{numberResult}'"), cancellationToken);
            }
            return await stepContext.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter another number.") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> WaterfallStep3(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Got '{numberResult}'."), cancellationToken);
            }
            return await stepContext.BeginAsync("TestComponentDialog", null, cancellationToken);
        }

        private class TestComponentDialog : ComponentDialog
        {
            public TestComponentDialog()
                : base("TestComponentDialog")
            {
                AddDialog(CreateWaterfall());
                AddDialog(new NumberPrompt<int>("number", defaultLocale: Culture.English));
            }
        }

        private class TestNestedComponentDialog : ComponentDialog
        {
            public TestNestedComponentDialog()
                : base("TestNestedComponentDialog")
            {
                AddDialog(new WaterfallDialog("test-waterfall", new WaterfallStep[] {
                    WaterfallStep1,
                    WaterfallStep2,
                    WaterfallStep3,
                }));
                AddDialog(new NumberPrompt<int>("number", defaultLocale: Culture.English));
                AddDialog(new TestComponentDialog());
            }
        }
    }
}
