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
    [TestCategory("Dimension Prompts")]
    public class DimensionPromptTests
    {
        [TestMethod]
        public async Task DimensionPrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var testPrompt = new DimensionPrompt(Culture.English);
                    if (!state.InPrompt)
                    {
                        state.InPrompt = true;
                        await testPrompt.Prompt(context, "Gimme:");
                    }
                    else
                    {
                        var dimensionResult = await testPrompt.Recognize(context);
                        if (dimensionResult.Succeeded())
                        {
                            Assert.IsTrue(dimensionResult.Value != float.NaN);
                            Assert.IsNotNull(dimensionResult.Text);
                            Assert.IsNotNull(dimensionResult.Unit);
                            Assert.IsInstanceOfType(dimensionResult.Value, typeof(float));
                            await context.SendActivity($"{dimensionResult.Value} {dimensionResult.Unit}");
                        }
                        else
                            await context.SendActivity(dimensionResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send("I am 4 feet wide")
                    .AssertReply("4 Foot")
                .Send(" it is 1 foot wide")
                    .AssertReply("1 Foot")
                .StartTest();
        }

        [TestMethod]
        public async Task DimensionPrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var numberPrompt = new DimensionPrompt(Culture.English, async (ctx, result) =>
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
                        var dimensionResult = await numberPrompt.Recognize(context);
                        if (dimensionResult.Succeeded())
                            await context.SendActivity($"{dimensionResult.Value} {dimensionResult.Unit}");
                        else
                            await context.SendActivity(dimensionResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send(" it is 1 foot wide")
                    .AssertReply(PromptStatus.TooSmall.ToString())
                .Send(" it is 40 feet wide")
                    .AssertReply("40 Foot")
                .StartTest();
        }

    }
}