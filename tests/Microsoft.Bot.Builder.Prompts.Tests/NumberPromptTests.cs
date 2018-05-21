// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Number Prompts")]
    public class NumberPromptTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task NumberPrompt_Float()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new NumberPrompt<float>(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await numberPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var numberResult = await numberPrompt.Recognize(context);
                    if (numberResult.Succeeded())
                    {
                        Assert.IsTrue(numberResult.Value != float.NaN);
                        Assert.IsNotNull(numberResult.Text);
                        Assert.IsInstanceOfType(numberResult.Value, typeof(float));
                        await context.SendActivity(numberResult.Value.ToString());
                    }
                    else
                        await context.SendActivity(numberResult.Status.ToString());
                }
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task NumberPrompt_Int()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new NumberPrompt<int>(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await numberPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var numberResult = await numberPrompt.Recognize(context);
                    if (numberResult.Succeeded())
                    {
                        Assert.IsInstanceOfType(numberResult.Value, typeof(int));
                        Assert.IsNotNull(numberResult.Text);
                        await context.SendActivity(numberResult.Value.ToString());
                    }
                    else
                        await context.SendActivity(numberResult.Status.ToString());
                }
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task NumberPrompt_Validator()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new NumberPrompt<int>(Culture.English, async (ctx, result) =>
                {
                    if (result.Value < 0)
                        result.Status = PromptStatus.TooSmall;
                    if (result.Value > 100)
                        result.Status = PromptStatus.TooBig;
                });
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await numberPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var numberResult = await numberPrompt.Recognize(context);
                    if (numberResult.Succeeded())
                    {
                        Assert.IsInstanceOfType(numberResult.Value, typeof(int));
                        Assert.IsTrue(numberResult.Value < 100);
                        Assert.IsNotNull(numberResult.Text);
                        await context.SendActivity(numberResult.Value.ToString());
                    }
                    else
                        await context.SendActivity(numberResult.Status.ToString());
                }
            });

            await flow.Test(activities).StartTest();
        }

    }
}