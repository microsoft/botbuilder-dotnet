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
    [TestCategory("Confirm Prompts")]
    public class ConfirmPromptTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task ConfirmPrompt_Test()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var testPrompt = new ConfirmPrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await testPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var confirmResult = await testPrompt.Recognize(context);
                    if (confirmResult.Succeeded())
                    {
                        Assert.IsNotNull(confirmResult.Text);
                        await context.SendActivity($"{confirmResult.Confirmation}");
                    }
                    else
                        await context.SendActivity(confirmResult.Status.ToString());
                }
            });

            await flow.Test(activities).StartTest();
        }

        [TestMethod]
        public async Task ConfirmPrompt_Validator()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var flow = new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var confirmPrompt = new ConfirmPrompt(Culture.English, async (ctx, result) =>
                {
                    if (ctx.Activity.Text.Contains("xxx"))
                        result.Status = PromptStatus.NotRecognized;
                });

                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await confirmPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var confirmResult = await confirmPrompt.Recognize(context);
                    if (confirmResult.Succeeded())
                        await context.SendActivity($"{confirmResult.Confirmation}");
                    else
                        await context.SendActivity(confirmResult.Status.ToString());
                }
            });

            await flow.Test(activities).StartTest();
        }

    }
}