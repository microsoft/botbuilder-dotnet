// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Temperature Prompts")]
    public class TemperaturePromptTests
    {
        [TestMethod]
        public async Task TemperaturePrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var testPrompt = new TemperaturePrompt(Culture.English);
                    if (!state.InPrompt)
                    {
                        state.InPrompt = true;
                        await testPrompt.Prompt(context, "Gimme:");
                    }
                    else
                    {
                        var tempResult = await testPrompt.Recognize(context);
                        if (tempResult.Succeeded())
                        {
                            Assert.IsTrue(tempResult.Value != float.NaN);
                            Assert.IsNotNull(tempResult.Text);
                            Assert.IsNotNull(tempResult.Unit);
                            Assert.IsInstanceOfType(tempResult.Value, typeof(float));
                            await context.SendActivity($"{tempResult.Value} {tempResult.Unit}");
                        }
                        else
                            await context.SendActivity(tempResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(" it is 43 degrees")
                    .AssertReply("43 Degree")
                .StartTest();
        }

        [TestMethod]
        public async Task TemperaturePrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new TemperaturePrompt(Culture.English, async (ctx, result) =>
                {
                    if (result.Value <= 10)
                        result.Status = PromptStatus.TooSmall;
                });
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await numberPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var tempResult = await numberPrompt.Recognize(context);
                    if (tempResult.Succeeded())
                        await context.SendActivity($"{tempResult.Value} {tempResult.Unit}");
                    else
                        await context.SendActivity(tempResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send(" it is 10 degrees")
                    .AssertReply(PromptStatus.TooSmall.ToString())
                .Send(" it is 43 degrees")
                    .AssertReply("43 Degree")
                .StartTest();
        }

    }
}