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
    [TestCategory("Ordinal Prompts")]
    public class OrdinalPromptTests
    {
        [TestMethod]
        public async Task OrdinalPrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var testPrompt = new OrdinalPrompt(Culture.English);
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
                            Assert.IsInstanceOfType(result.Value, typeof(int));
                            context.Reply(result.Value.ToString());
                        }
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply("null")
                .Send(" the second one please ")
                    .AssertReply("2")
                .StartTest();
        }

        [TestMethod]
        public async Task OrdinalPrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new OrdinalPrompt(Culture.English, async (ctx, result) =>  result.Value > 2);
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
                    {
                        Assert.IsInstanceOfType(result.Value, typeof(int));
                        Assert.IsTrue(result.Value < 100);
                        Assert.IsNotNull(result.Text);
                        context.Reply(result.Value.ToString());
                    }
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("the first one")
                    .AssertReply("null")
                .Send("the third one")
                    .AssertReply("3")
                .StartTest();
        }

    }
}