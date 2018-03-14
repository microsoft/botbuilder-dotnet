// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Age Prompts")]
    public class AgePromptTests
    {
        [TestMethod]
        public async Task AgePrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));
                
            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var testPrompt = new AgePrompt(Culture.English);
                    if (!state.InPrompt)
                    {
                        state.InPrompt = true;
                        await testPrompt.Prompt(context, "Gimme:");
                    }
                    else
                    {
                        var ageResult = await testPrompt.Recognize(context);
                        if (ageResult.Succeeded())
                        {
                            Assert.IsTrue(ageResult.Value != float.NaN);
                            Assert.IsNotNull(ageResult.Text);
                            Assert.IsNotNull(ageResult.Unit);
                            Assert.IsInstanceOfType(ageResult.Value, typeof(float));
                            await context.SendActivity($"{ageResult.Value} {ageResult.Unit}");
                        }
                        else
                            await context.SendActivity(ageResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send("I am 30 years old")
                    .AssertReply("30 Year")
                .StartTest();
        }

        [TestMethod]
        public async Task AgePrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new AgePrompt(Culture.English, async (ctx, result) =>
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
                    var numberResult = await numberPrompt.Recognize(context);
                    if (numberResult.Succeeded())
                        await context.SendActivity($"{numberResult.Value} {numberResult.Unit}");
                    else
                        await context.SendActivity(numberResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send(" it is 1 year old")
                    .AssertReply(PromptStatus.TooSmall.ToString())
                .Send(" it is 15 year old")
                    .AssertReply("15 Year")
                .StartTest();
        }

    }
}