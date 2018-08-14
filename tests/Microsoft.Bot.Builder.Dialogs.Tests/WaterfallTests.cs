// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class WaterfallTests
    {
        [TestMethod]
        public async Task Waterfall()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(convoState);

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
            {
                async (dc, step) => { await dc.Context.SendActivityAsync("step1"); return Dialog.EndOfTurn; },
                async (dc, step) => { await dc.Context.SendActivityAsync("step2"); return Dialog.EndOfTurn; },
                async (dc, step) => { await dc.Context.SendActivityAsync("step3"); return Dialog.EndOfTurn; },
            }));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);
                await dc.ContinueAsync();
                if (!turnContext.Responded)
                {
                    await dc.BeginAsync("test");
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .Send("hello")
            .AssertReply("step2")
            .Send("hello")
            .AssertReply("step3")
            .StartTestAsync();
        }

        
        [TestMethod]
        public async Task WaterfallPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(dialogState);
                dialogs.Add(Create_Waterfall2());
                var numberPrompt = new NumberPrompt<int>("number", defaultLocale: Culture.English);
                dialogs.Add(numberPrompt);

                var dc = await dialogs.CreateContextAsync(turnContext);

                await dc.ContinueAsync();

                if (!turnContext.Responded)
                {
                    await dc.BeginAsync("test-waterfall");
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .AssertReply("Enter a number.")
            .Send("hello again")
            .AssertReply("It must be a number")
            .Send("42")
            .AssertReply("Thanks for '42'")
            .AssertReply("step2")
            .AssertReply("Enter a number.")
            .Send("apple")
            .AssertReply("It must be a number")
            .Send("orange")
            .AssertReply("It must be a number")
            .Send("64")
            .AssertReply("Thanks for '64'")
            .AssertReply("step3")
            .StartTestAsync();
        }

        private static WaterfallDialog Create_Waterfall2()
        {
            return new WaterfallDialog("test-waterfall", new WaterfallStep[] {
                Waterfall2_Step1,
                Waterfall2_Step2,
                Waterfall2_Step3
            });
        }

        private static async Task<DialogTurnResult> Waterfall2_Step1(DialogContext dc, WaterfallStepContext stepContext)
        {
            await dc.Context.SendActivityAsync("step1");
            return await dc.PromptAsync("number", new PromptOptions {
                Prompt = MessageFactory.Text("Enter a number."),
                RetryPrompt = MessageFactory.Text("It must be a number")
            });
        }
        private static async Task<DialogTurnResult> Waterfall2_Step2(DialogContext dc, WaterfallStepContext stepContext)
        {
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await dc.Context.SendActivityAsync($"Thanks for '{numberResult}'");
            }
            await dc.Context.SendActivityAsync("step2");
            return await dc.PromptAsync("number",
                new PromptOptions {
                    Prompt = MessageFactory.Text("Enter a number."),
                    RetryPrompt = MessageFactory.Text("It must be a number")
                });
        }
        private static async Task<DialogTurnResult> Waterfall2_Step3(DialogContext dc, WaterfallStepContext stepContext)
        {
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await dc.Context.SendActivityAsync($"Thanks for '{numberResult}'");
            }
            await dc.Context.SendActivityAsync("step3");
            return await dc.EndAsync(new Dictionary<string, object> { { "Value", "All Done!" } });
        }

        [TestMethod]
        public async Task WaterfallNested()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dialogState = convoState.CreateProperty<DialogState>("dialogState");
                var dialogs = new DialogSet(dialogState);
                dialogs.Add(Create_Waterfall3());
                dialogs.Add(Create_Waterfall4());
                dialogs.Add(Create_Waterfall5());

                var dc = await dialogs.CreateContextAsync(turnContext);

                await dc.ContinueAsync();

                if (!turnContext.Responded)
                {
                    await dc.BeginAsync("test-waterfall-a");
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .AssertReply("step1.1")
            .Send("hello")
            .AssertReply("step1.2")
            .Send("hello")
            .AssertReply("step2")
            .AssertReply("step2.1")
            .Send("hello")
            .AssertReply("step2.2")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task WaterfallDateTimePromptFirstInvalidThenValidInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new DateTimePrompt("dateTimePrompt", defaultLocale: Culture.English));
            dialogs.Add(new WaterfallDialog("test-dateTimePrompt", new WaterfallStep[]
            {
                async (dc, args) =>
                {
                    return await dc.PromptAsync("dateTimePrompt", "Provide a date");
                },
                async (dc, args) =>
                {
                    Assert.IsNotNull(args);
                    return await dc.EndAsync();
                }
            }));

            var adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());

                var dc = await dialogs.CreateContextAsync(turnContext);

                await dc.ContinueAsync();

                if (!turnContext.Responded)
                {
                    await dc.BeginAsync("test-dateTimePrompt");
                }
            })
            .Send("hello")
            .AssertReply("Provide a date")
            .Send("hello again")
            .AssertReply("Provide a date")
            .Send("Wednesday 4 oclock")
            .StartTestAsync();
        }

        private static WaterfallDialog Create_Waterfall3()
        {
            return new WaterfallDialog("test-waterfall-a", new WaterfallStep[] {
                Waterfall3_Step1,
                Waterfall3_Step2
            });
        }
        private static WaterfallDialog Create_Waterfall4()
        {
            return new WaterfallDialog("test-waterfall-b", new WaterfallStep[] {
                Waterfall4_Step1,
                Waterfall4_Step2
            });
        }

        private static WaterfallDialog Create_Waterfall5()
        {
            return new WaterfallDialog("test-waterfall-c", new WaterfallStep[] {
                Waterfall5_Step1,
                Waterfall5_Step2
            });
        }

        private static async Task<DialogTurnResult> Waterfall3_Step1(DialogContext dc, WaterfallStepContext stepContext)
        {
            await dc.Context.SendActivityAsync("step1");
            return await dc.BeginAsync("test-waterfall-b");
        }
        private static async Task<DialogTurnResult> Waterfall3_Step2(DialogContext dc, WaterfallStepContext stepContext)
        {
            await dc.Context.SendActivityAsync("step2");
            return await dc.BeginAsync("test-waterfall-c");
        }

        private static async Task<DialogTurnResult> Waterfall4_Step1(DialogContext dc, WaterfallStepContext stepContext)
        {
            await dc.Context.SendActivityAsync("step1.1");
            return Dialog.EndOfTurn;
        }
        private static async Task<DialogTurnResult> Waterfall4_Step2(DialogContext dc, WaterfallStepContext stepContext)
        {
            await dc.Context.SendActivityAsync("step1.2");
            return Dialog.EndOfTurn;
        }

        private static async Task<DialogTurnResult> Waterfall5_Step1(DialogContext dc, WaterfallStepContext stepContext)
        {
            await dc.Context.SendActivityAsync("step2.1");
            return Dialog.EndOfTurn;
        }
        private static async Task<DialogTurnResult> Waterfall5_Step2(DialogContext dc, WaterfallStepContext stepContext)
        {
            await dc.Context.SendActivityAsync("step2.2");
            return await dc.EndAsync();
        }

    }
}
