// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("DateTime Prompts")]

    public class DateTimePromptTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task DateTimePrompt_ShouldSendPrompt()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var flow = new TestFlow(new TestAdapter(), async (context) =>
            {
                var dateTimePrompt = new DateTimePrompt(Culture.English);
                await dateTimePrompt.Prompt(context, "What date would you like?");
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task DateTimePrompt_ShouldRecognizeDateTime_Value()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var dateTimePrompt = new DateTimePrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await dateTimePrompt.Prompt(context, "What date would you like?");
                }
                else
                {
                    var dateTimeResult = await dateTimePrompt.Recognize(context);
                    if (dateTimeResult.Succeeded())
                    {
                        var resolution = dateTimeResult.Resolution.First();
                        var reply = $"Timex:'{resolution.Timex}' Value:'{resolution.Value}'";
                        await context.SendActivity(reply);
                    }
                    else
                    {
                        await context.SendActivity(dateTimeResult.Status.ToString());
                    }
                }
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task DateTimePrompt_ShouldRecognizeDateTime_Range()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var dateTimePrompt = new DateTimePrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await dateTimePrompt.Prompt(context, "What window would you like to book?");
                }
                else
                {
                    var dateTimeResult = await dateTimePrompt.Recognize(context);
                    if (dateTimeResult.Succeeded())
                    {
                        var resolution = dateTimeResult.Resolution.First();
                        var reply = $"Timex:'{resolution.Timex}'";
                        await context.SendActivity(reply);
                    }
                    else
                    {
                        await context.SendActivity(dateTimeResult.Status.ToString());
                    }
                }
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task DateTimePrompt_ShouldRecognizeDateTime_StartEnd()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var dateTimePrompt = new DateTimePrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await dateTimePrompt.Prompt(context, "What month are you interested in?");
                }
                else
                {
                    var dateTimeResult = await dateTimePrompt.Recognize(context);
                    if (dateTimeResult.Succeeded())
                    {
                        var resolution = dateTimeResult.Resolution.First();
                        var reply = $"Timex:'{resolution.Timex}' Start:'{resolution.Start}' End:'{resolution.End}'";
                        await context.SendActivity(reply);
                    }
                    else
                    {
                        await context.SendActivity(dateTimeResult.Status.ToString());
                    }
                }
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task DateTimePrompt_ShouldRecognizeDateTime_CustomValidator()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            var adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            PromptValidator<DateTimeResult> validator = (context, result) =>
            {
                result.Status = PromptStatus.NotRecognized;
                return Task.CompletedTask;
            };

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);

                var dateTimePrompt = new DateTimePrompt(Culture.English, validator);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await dateTimePrompt.Prompt(context, "What date would you like?");
                }
                else
                {
                    var dateTimeResult = await dateTimePrompt.Recognize(context);
                    if (dateTimeResult.Succeeded())
                    {
                        var resolution = dateTimeResult.Resolution.First();
                        var reply = $"Timex:'{resolution.Timex}' Value:'{resolution.Value}'";
                        await context.SendActivity(reply);
                    }
                    else
                    {
                        await context.SendActivity(dateTimeResult.Status.ToString());
                    }
                }
            });

            await flow.Test(activities).StartTest();
        }
    }
}
