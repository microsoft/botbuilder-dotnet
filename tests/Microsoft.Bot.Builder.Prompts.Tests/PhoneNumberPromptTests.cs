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
                            context.Reply($"{phoneResult.Value}");
                        }
                        else
                            context.Reply(phoneResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(RecognitionStatus.NotRecognized.ToString())
                .Send("123 123123sdfsdf 123 1asdf23123 123 ")
                    .AssertReply(RecognitionStatus.NotRecognized.ToString())
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
                        result.Status = RecognitionStatus.OutOfRange;
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
                        context.Reply($"{phoneResult.Value}");
                    else
                        context.Reply(phoneResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("888-123-4567")
                    .AssertReply(RecognitionStatus.OutOfRange.ToString())
                .Send("123-123-4567")
                    .AssertReply("123-123-4567")
                .StartTest();
        }

    }
}