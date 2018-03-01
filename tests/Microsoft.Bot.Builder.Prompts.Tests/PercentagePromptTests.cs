// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Percentage Prompts")]
    public class PercentagePromptTests
    {
        [TestMethod]
        public async Task PercentagePrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var testPrompt = new PercentagePrompt(Culture.English);
                    if (!state.InPrompt)
                    {
                        state.InPrompt = true;
                        await testPrompt.Prompt(context, "Gimme:");
                    }
                    else
                    {
                        var result = await testPrompt.Recognize(context);
                        if (result == null)
                            context.Reply("null");
                        else
                        {
                            Assert.IsTrue(result.Value != float.NaN);
                            Assert.IsNotNull(result.Text);
                            Assert.IsInstanceOfType(result.Value, typeof(float));
                            context.Reply($"{result.Value}");
                        }
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply("null")
                .Send("give me 5")
                    .AssertReply("null")
                .Send(" I would like forty five percent")
                    .AssertReply("45")
                .StartTest();
        }

        [TestMethod]
        public async Task PercentagePrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new PercentagePrompt(Culture.English, async (ctx, result) =>  result.Value > 10);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await numberPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var result = await numberPrompt.Recognize(context);
                    if (result == null)
                        context.Reply("null");
                    else
                        context.Reply($"{result.Value}");
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send(" I would like 5%")
                    .AssertReply("null")
                .Send(" I would like 30%")
                    .AssertReply("30")
                .StartTest();
        }

    }
}