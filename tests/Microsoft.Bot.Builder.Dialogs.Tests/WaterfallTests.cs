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
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var waterfall = new Waterfall(new WaterfallStep[]
                {
                    async (dc, args, next) => { await dc.Context.SendActivityAsync("step1"); },
                    async (dc, args, next) => { await dc.Context.SendActivityAsync("step2"); },
                    async (dc, args, next) => { await dc.Context.SendActivityAsync("step3"); },
                });

                var dialogCompletion = await waterfall.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await waterfall.BeginAsync(turnContext, state);
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
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var dialogs = new DialogSet();
                dialogs.Add("test-waterfall", Create_Waterfall2());
                dialogs.Add("number", new NumberPrompt<int>(Culture.English));

                var dc = dialogs.CreateContext(turnContext, state);

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

        private static WaterfallStep[] Create_Waterfall2()
        {
            return new WaterfallStep[] {
                Waterfall2_Step1,
                Waterfall2_Step2,
                Waterfall2_Step3
            };
        }

        private static async Task Waterfall2_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivityAsync("step1");
            await dc.PromptAsync("number", "Enter a number.", new PromptOptions { RetryPromptString = "It must be a number" });
        }
        private static async Task Waterfall2_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            if (args != null)
            {
                var numberResult = (NumberResult<int>)args;
                await dc.Context.SendActivityAsync($"Thanks for '{numberResult.Value}'");
            }
            await dc.Context.SendActivityAsync("step2");
            await dc.PromptAsync("number", "Enter a number.", new PromptOptions { RetryPromptString = "It must be a number" });
        }
        private static async Task Waterfall2_Step3(DialogContext dc, object args, SkipStepFunction next)
        {
            if (args != null)
            {
                var numberResult = (NumberResult<int>)args;
                await dc.Context.SendActivityAsync($"Thanks for '{numberResult.Value}'");
            }
            await dc.Context.SendActivityAsync("step3");
            await dc.EndAsync(new Dictionary<string, object> { { "Value", "All Done!" } });
        }

        [TestMethod]
        public async Task WaterfallNested()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var testProperty = convoState.CreateProperty<Dictionary<string, object>>("test");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());
                var dialogs = new DialogSet();
                dialogs.Add("test-waterfall-a", Create_Waterfall3());
                dialogs.Add("test-waterfall-b", Create_Waterfall4());
                dialogs.Add("test-waterfall-c", Create_Waterfall5());

                var dc = dialogs.CreateContext(turnContext, state);

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
            var testProperty = convoState.CreateProperty <Dictionary<string, object>>("test");

            var dialogs = new DialogSet();
            dialogs.Add("dateTimePrompt", new DateTimePrompt(Culture.English));
            dialogs.Add("test-dateTimePrompt", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    await dc.PromptAsync("dateTimePrompt", "Provide a date");
                },
                async (dc, args, next) =>
                {
                    Assert.IsNotNull(args);
                    await dc.EndAsync();
                }
            });

            var adapter = new TestAdapter()
                .Use(convoState);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await testProperty.GetAsync(turnContext, () => new Dictionary<string, object>());

                var dc = dialogs.CreateContext(turnContext, state);

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

        private static WaterfallStep[] Create_Waterfall3()
        {
            return new WaterfallStep[] {
                Waterfall3_Step1,
                Waterfall3_Step2
            };
        }
        private static WaterfallStep[] Create_Waterfall4()
        {
            return new WaterfallStep[] {
                Waterfall4_Step1,
                Waterfall4_Step2
            };
        }

        private static WaterfallStep[] Create_Waterfall5()
        {
            return new WaterfallStep[] {
                Waterfall5_Step1,
                Waterfall5_Step2
            };
        }

        private static async Task Waterfall3_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivityAsync("step1");
            await dc.BeginAsync("test-waterfall-b");
        }
        private static async Task Waterfall3_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivityAsync("step2");
            await dc.BeginAsync("test-waterfall-c");
        }

        private static async Task Waterfall4_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivityAsync("step1.1");
        }
        private static async Task Waterfall4_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivityAsync("step1.2");
        }

        private static async Task Waterfall5_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivityAsync("step2.1");
        }
        private static async Task Waterfall5_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivityAsync("step2.2");
            await dc.EndAsync();
        }
    }
}
