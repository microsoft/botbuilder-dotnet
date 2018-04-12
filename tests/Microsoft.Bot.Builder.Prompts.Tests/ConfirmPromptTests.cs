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
    [TestCategory("Confirm Prompts")]
    public class ConfirmPromptTests
    {
        [TestMethod]
        public async Task ConfirmPrompt_Test()
        {
            TestAdapter adapter = new TestAdapter();

            var inPrompt = false;

            await new TestFlow(adapter, async (context) =>
                {
                    var testPrompt = new ConfirmPrompt(Culture.English);
                    if (!inPrompt)
                    {
                        inPrompt = true;
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
                .AssertReply("Gimme:")
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
            TestAdapter adapter = new TestAdapter();

            var inPrompt = false;

            await new TestFlow(adapter, async (context) =>
            {
                var confirmPrompt = new ConfirmPrompt(Culture.English, async (ctx, result) =>
                {
                    if (ctx.Activity.Text.Contains("xxx"))
                        result.Status = PromptStatus.NotRecognized;
                });

                if (!inPrompt)
                {
                    inPrompt = true;
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
                .AssertReply("Gimme:")
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

    }
}