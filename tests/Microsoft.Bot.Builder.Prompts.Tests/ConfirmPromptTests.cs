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
    [TestCategory("Confirm Prompts")]
    public class ConfirmPromptTests
    {
        [TestMethod]
        public async Task ConfirmPrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var testPrompt = new ConfirmPrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await testPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var confirmResult = await testPrompt.Recognize(context);
                    if (confirmResult.Succeeded())
                    {
                        Assert.IsNotNull(confirmResult.Text);
                        await context.SendActivity($"{confirmResult.Confirmation}");
                    }
                    else
                        await context.SendActivity(confirmResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme: (1) Yes or (2) No")
                .Send("tyest tnot")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(".. yes please ")
                    .AssertReply("True")
                .Send(".. no thank you")
                    .AssertReply("False")
                .StartTest();
        }

        [TestMethod]
        public async Task ConfirmPrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var confirmPrompt = new ConfirmPrompt(Culture.English, async (ctx, result) =>
                {
                    if (ctx.Activity.Text.Contains("xxx"))
                        result.Status = PromptStatus.NotRecognized;
                });

                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await confirmPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var confirmResult = await confirmPrompt.Recognize(context);
                    if (confirmResult.Succeeded())
                        await context.SendActivity($"{confirmResult.Confirmation}");
                    else
                        await context.SendActivity(confirmResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme: (1) Yes or (2) No")
                .Send(" yes you xxx")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(" no way you xxx")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(" yep")
                    .AssertReply("True")
                .Send(" nope")
                    .AssertReply("False")
                .StartTest();
        }


        [TestMethod]
        public async Task ConfirmPrompt_StyleTest()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var testPrompt = new ConfirmPrompt(culture: Culture.English, listStyle: Choices.ListStyle.Inline);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await testPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var confirmResult = await testPrompt.Recognize(context);
                    if (confirmResult.Succeeded())
                    {
                        Assert.IsNotNull(confirmResult.Text);
                        await context.SendActivity($"{confirmResult.Confirmation}");
                    }
                    else
                        await context.SendActivity(confirmResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme: (1) Yes or (2) No")
                .Send("tyest tnot")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(".. yes please ")
                    .AssertReply("True")
                .Send(".. no thank you")
                    .AssertReply("False")
                .StartTest();
        }

        [TestMethod]
        public async Task ConfirmPrompt_StyleListTest()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var testPrompt = new ConfirmPrompt(culture: Culture.English, listStyle: Choices.ListStyle.List);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await testPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var confirmResult = await testPrompt.Recognize(context);
                    if (confirmResult.Succeeded())
                    {
                        Assert.IsNotNull(confirmResult.Text);
                        await context.SendActivity($"{confirmResult.Confirmation}");
                    }
                    else
                        await context.SendActivity(confirmResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:\n\n   1. Yes\n   2. No")
                .Send("tyest tnot")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(".. yes please ")
                    .AssertReply("True")
                .Send(".. no thank you")
                    .AssertReply("False")
                .StartTest();
        }

        [TestMethod]
        public async Task ConfirmPrompt_CultureTest()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var testPrompt = new ConfirmPrompt(culture: Culture.Spanish);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await testPrompt.Prompt(context, "Confirma:");
                }
                else
                {
                    var confirmResult = await testPrompt.Recognize(context);
                    if (confirmResult.Succeeded())
                    {
                        Assert.IsNotNull(confirmResult.Text);
                        await context.SendActivity($"{confirmResult.Confirmation}");
                    }
                    else
                        await context.SendActivity(confirmResult.Status.ToString());
                }
            })
                .Send("Hola!")
                .AssertReply("Confirma: (1) Sí o (2) No")
                .Send("tsít tno")
                    .AssertReply(PromptStatus.NotRecognized.ToString())
                .Send(".. sí por favor ")
                    .AssertReply("True")
                .Send(".. no gracias")
                    .AssertReply("False")
                .StartTest();
        }
    }
}