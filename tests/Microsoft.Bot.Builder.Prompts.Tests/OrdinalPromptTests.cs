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
                        var ordinalResult = await testPrompt.Recognize(context);
                        if (ordinalResult.Succeeded())
                        {
                            Assert.IsTrue(ordinalResult.Value != float.NaN);
                            Assert.IsNotNull(ordinalResult.Text);
                            Assert.IsInstanceOfType(ordinalResult.Value, typeof(int));
                            await context.SendActivity(ordinalResult.Value.ToString());
                        }
                        else
                            await context.SendActivity(ordinalResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
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
                var numberPrompt = new OrdinalPrompt(Culture.English, async (ctx, result) =>
                {
                    if (result.Value <= 2)
                        result.Status = PromptStatus.TooSmall;
                });
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await numberPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var ordinalResult = await numberPrompt.Recognize(context);
                    if (ordinalResult.Succeeded())
                    {
                        Assert.IsInstanceOfType(ordinalResult.Value, typeof(int));
                        Assert.IsTrue(ordinalResult.Value < 100);
                        Assert.IsNotNull(ordinalResult.Text);
                        await context.SendActivity(ordinalResult.Value.ToString());
                    }
                    else
                        await context.SendActivity(ordinalResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("the first one")
                    .AssertReply(PromptStatus.TooSmall.ToString())
                .Send("the third one")
                    .AssertReply("3")
                .StartTest();
        }

    }
}