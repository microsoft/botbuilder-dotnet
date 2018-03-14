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
    [TestCategory("PhoneNumber Prompts")]
    public class PhoneNumberPromptTests
    {
        [TestMethod]
        public async Task PhoneNumberPrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var testPrompt = new PhoneNumberPrompt(Culture.English);
                    if (!state.InPrompt)
                    {
                        state.InPrompt = true;
                        await testPrompt.Prompt(context, "Gimme:");
                    }
                    else
                    {
                        var phoneResult = await testPrompt.Recognize(context);
                        if (phoneResult.Succeeded())
                        {
                            Assert.IsNotNull(phoneResult.Text);
                            Assert.IsNotNull(phoneResult.Value);
                            await context.SendActivity($"{phoneResult.Value}");
                        }
                        else
                            await context.SendActivity(phoneResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send("123 123123sdfsdf 123 1asdf23123 123 ")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send("123-456-7890")
                    .AssertReply("123-456-7890")
                .StartTest();
        }

        [TestMethod]
        public async Task PhoneNumberPrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new PhoneNumberPrompt(Culture.English, async (ctx, result) =>
                {
                    if (!result.Value.StartsWith("123"))
                        result.Status = PromptStatus.OutOfRange;
                });
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await numberPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var phoneResult = await numberPrompt.Recognize(context);
                    if (phoneResult.Succeeded())
                        await context.SendActivity($"{phoneResult.Value}");
                    else
                        await context.SendActivity(phoneResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("888-123-4567")
                    .AssertReply(PromptStatus.OutOfRange.ToString())
                .Send("123-123-4567")
                    .AssertReply("123-123-4567")
                .StartTest();
        }

    }
}