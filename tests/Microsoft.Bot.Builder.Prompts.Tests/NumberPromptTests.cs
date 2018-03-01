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
    public class TestState : StoreItem
    {
        public bool InPrompt { get; set; } = false;
    }

    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Number Prompts")]
    public class NumberPromptTests
    {
        [TestMethod]
        public async Task NumberPrompt_Float()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
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
                        var result = await numberPrompt.Recognize(context);
                        if (result == null)
                            context.Reply("null");
                        else
                        {
                            Assert.IsTrue(result.Value != float.NaN);
                            Assert.IsNotNull(result.Text);
                            Assert.IsInstanceOfType(result.Value, typeof(float));
                            context.Reply(result.Value.ToString());
                        }
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply("null")
                .Send("asdf df 123")
                    .AssertReply("123")
                .Send(" asdf asd 123.43 adsfsdf ")
                    .AssertReply("123.43")
                .StartTest();
        }

        [TestMethod]
        public async Task NumberPrompt_Int()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
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
                    var result = await numberPrompt.Recognize(context);
                    if (result == null)
                        context.Reply("null");
                    else
                    {
                        Assert.IsInstanceOfType(result.Value, typeof(int));
                        Assert.IsNotNull(result.Text);
                        context.Reply(result.Value.ToString());
                    }
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply("null")
                .Send("asdf df 123")
                    .AssertReply("123")
                .Send(" asdf asd 123.43 adsfsdf ")
                    .AssertReply("null")
                .StartTest();
        }

        [TestMethod]
        public async Task NumberPrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new NumberPrompt<int>(Culture.English, async (ctx, result) =>  result.Value < 100);
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
                .Send("asdf df 123")
                    .AssertReply("null")
                .Send(" asdf asd 12 adsfsdf ")
                    .AssertReply("12")
                .StartTest();
        }

    }
}