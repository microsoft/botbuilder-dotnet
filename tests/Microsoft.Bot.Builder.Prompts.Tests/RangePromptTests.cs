// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.State;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Range Prompts")]
    public class RangePromptTests
    {
        [TestMethod]
        public async Task RangePrompt_Test()
        {
            TestAdapter adapter = new TestAdapter();

            var inPrompt = false;

            await new TestFlow(adapter, async (context) =>
                {
                    var testPrompt = new RangePrompt<int>(Culture.English);
                    if (!inPrompt)
                    {
                        inPrompt = true;
                        await testPrompt.Prompt(context, "Gimme:");
                    }
                    else
                    {
                        var rangeResult = await testPrompt.Recognize(context);
                        if (rangeResult.Succeeded())
                        {
                            Assert.IsTrue(rangeResult.Start > 0);
                            Assert.IsTrue(rangeResult.End > rangeResult.Start);
                            Assert.IsNotNull(rangeResult.Text);
                            await context.SendActivity($"{rangeResult.Start}-{rangeResult.End}");
                        }
                        else
                            await context.SendActivity(rangeResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send("give me 5 10")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(" give me between 5 and 10")
                    .AssertReply("5-10")
                .StartTest();
        }

        [TestMethod]
        public async Task RangePrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter();

            var inPrompt = false;

            await new TestFlow(adapter, async (context) =>
            {
                var testPrompt = new RangePrompt<int>(Culture.English, async (c, result) =>
                {
                    if (result.End - result.Start <= 5)
                        result.Status = PromptStatus.OutOfRange;
                });
                if (!inPrompt)
                {
                    inPrompt = true;
                    await testPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var rangeResult = await testPrompt.Recognize(context);
                    if (rangeResult.Succeeded())
                    {
                        Assert.IsTrue(rangeResult.Start > 0);
                        Assert.IsTrue(rangeResult.End > rangeResult.Start);
                        Assert.IsNotNull(rangeResult.Text);
                        await context.SendActivity($"{rangeResult.Start}-{rangeResult.End}");
                    }
                    else
                        await context.SendActivity(rangeResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("give me between 1 and 4")
                    .AssertReply(PromptStatus.OutOfRange.ToString())
                .Send(" give me between 1 and 10")
                    .AssertReply("1-10")
                .StartTest();
        }

    }
}