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
                        var result = await testPrompt.Recognize(context);
                        if (result == null)
                            context.Reply("null");
                        else
                        {
                            Assert.IsTrue(result.Value != float.NaN);
                            Assert.IsNotNull(result.Text);
                            Assert.IsNotNull(result.Unit);
                            Assert.IsInstanceOfType(result.Value, typeof(float));
                            context.Reply($"{result.Value} {result.Unit}");
                        }
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply("null")
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
                var numberPrompt = new AgePrompt(Culture.English, async (ctx, result) =>  result.Value > 10);
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
                        context.Reply($"{result.Value} {result.Unit}");
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send(" it is 1 year old")
                    .AssertReply("null")
                .Send(" it is 15 year old")
                    .AssertReply("15 Year")
                .StartTest();
        }

    }
}