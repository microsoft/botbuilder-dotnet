using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Transcripts.Tests
{
    [TestClass]
    public class DialogsTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task AttachmentPrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new AttachmentPrompt();

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "please add an attachment." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var attachmentResult = (AttachmentResult)dialogCompletion.Result;
                    var reply = (string)attachmentResult.Attachments.First().Content;
                    await turnContext.SendActivity(reply);
                }
            })
            .Test(activities)
            .StartTest();
        }

        [TestMethod]
        public async Task ChoicePrompt()
        {
            var dialogs = new DialogSet();

            dialogs.Add("test-prompt", new Dialogs.ChoicePrompt(Culture.English) { Style = ListStyle.Inline });

            var promptOptions = new ChoicePromptOptions
            {
                Choices = new List<Choice>
                {
                    new Choice { Value = "red" },
                    new Choice { Value = "green" },
                    new Choice { Value = "blue" },
                },
                RetryPromptString = "I didn't catch that. Select a color from the list."
            };

            dialogs.Add("test",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        await dc.Prompt("test-prompt", "favorite color?", promptOptions);
                    },
                    async (dc, args, next) =>
                    {
                        var choiceResult = (ChoiceResult)args;
                        await dc.Context.SendActivity($"Bot received the choice '{choiceResult.Value.Value}'.");
                        await dc.End();
                    }
                }
            );

            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();

                if (!turnContext.Responded)
                {
                    await dc.Begin("test");
                }
            })
            .Test(activities)
            .StartTest();
        }

        [TestMethod]
        public async Task ConfirmPrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ConfirmPrompt(Culture.English) { Style = ListStyle.None };

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Please confirm.",
                            RetryPromptString = "Please confirm, say 'yes' or 'no' or something like that."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    if (((ConfirmResult)dialogCompletion.Result).Confirmation)
                    {
                        await turnContext.SendActivity("Confirmed.");
                    }
                    else
                    {
                        await turnContext.SendActivity("Not confirmed.");
                    }
                }
            })
            .Test(activities)
            .StartTest();
        }

        [TestMethod]
        public async Task DateTimePrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new DateTimePrompt(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "What date would you like?", RetryPromptString = "Sorry, but that is not a date. What date would you like?" });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var dateTimeResult = (DateTimeResult)dialogCompletion.Result;
                    var resolution = dateTimeResult.Resolution.First();
                    var reply = $"Timex:'{resolution.Timex}' Value:'{resolution.Value}'";
                    await turnContext.SendActivity(reply);
                }
            })
            .Test(activities)
            .StartTest();
        }

        [TestMethod]
        public async Task NumberPrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            PromptValidatorEx.PromptValidator<NumberResult<int>> validator = async (ctx, result) =>
            {
                if (result.Value < 0)
                    result.Status = PromptStatus.TooSmall;
                if (result.Value > 100)
                    result.Status = PromptStatus.TooBig;
                await Task.CompletedTask;
            };

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new NumberPrompt<int>(Culture.English, validator);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Enter a number.",
                            RetryPromptString = "You must enter a valid positive number less than 100."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var numberResult = (NumberResult<int>)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the number '{numberResult.Value}'.");
                }
            })
            .Test(activities)
            .StartTest();
        }

        [TestMethod]
        public async Task TextPrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            PromptValidatorEx.PromptValidator<TextResult> validator = async (ctx, result) =>
            {
                if (result.Value.Length <= 3)
                    result.Status = PromptStatus.TooSmall;
                await Task.CompletedTask;
            };

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new TextPrompt(validator);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Enter some text.",
                            RetryPromptString = "Make sure the text is greater than three characters."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    var textResult = (TextResult)dialogCompletion.Result;
                    await turnContext.SendActivity($"Bot received the text '{textResult.Value}'.");
                }
            })
            .Test(activities)
            .StartTest();
        }

        [TestMethod]
        public async Task Waterfall()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var waterfall = new Waterfall(new WaterfallStep[]
                {
                    async (dc, args, next) => { await dc.Context.SendActivity("step1"); },
                    async (dc, args, next) => { await dc.Context.SendActivity("step2"); },
                    async (dc, args, next) => { await dc.Context.SendActivity("step3"); },
                });

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);

                var dialogCompletion = await waterfall.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await waterfall.Begin(turnContext, state);
                }
            })
            .Test(activities)
            .StartTest();
        }

        [TestMethod]
        public async Task WaterfallPrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-waterfall", Create_Waterfall2());
                dialogs.Add("number", new NumberPrompt<int>(Culture.English));

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();

                if (!turnContext.Responded)
                {
                    await dc.Begin("test-waterfall");
                }
            })
            .Test(activities)
            .StartTest();
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
            await dc.Context.SendActivity("step1");
            await dc.Prompt("number", "Enter a number.", new PromptOptions { RetryPromptString = "It must be a number" });
        }
        private static async Task Waterfall2_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            if (args != null)
            {
                var numberResult = (NumberResult<int>)args;
                await dc.Context.SendActivity($"Thanks for '{numberResult.Value}'");
            }
            await dc.Context.SendActivity("step2");
            await dc.Prompt("number", "Enter a number.", new PromptOptions { RetryPromptString = "It must be a number" });
        }
        private static async Task Waterfall2_Step3(DialogContext dc, object args, SkipStepFunction next)
        {
            if (args != null)
            {
                var numberResult = (NumberResult<int>)args;
                await dc.Context.SendActivity($"Thanks for '{numberResult.Value}'");
            }
            await dc.Context.SendActivity("step3");
            await dc.End(new Dictionary<string, object> { { "Value", "All Done!" } });
        }

        [TestMethod]
        public async Task WaterfallNested()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var dialogs = new DialogSet();
                dialogs.Add("test-waterfall-a", Create_Waterfall3());
                dialogs.Add("test-waterfall-b", Create_Waterfall4());
                dialogs.Add("test-waterfall-c", Create_Waterfall5());

                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);

                await dc.Continue();

                if (!turnContext.Responded)
                {
                    await dc.Begin("test-waterfall-a");
                }
            })
            .Test(activities)
            .StartTest();
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
            await dc.Context.SendActivity("step1");
            await dc.Begin("test-waterfall-b");
        }
        private static async Task Waterfall3_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2");
            await dc.Begin("test-waterfall-c");
        }

        private static async Task Waterfall4_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1.1");
        }
        private static async Task Waterfall4_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step1.2");
        }

        private static async Task Waterfall5_Step1(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2.1");
        }
        private static async Task Waterfall5_Step2(DialogContext dc, object args, SkipStepFunction next)
        {
            await dc.Context.SendActivity("step2.2");
            await dc.End();
        }
    }
}