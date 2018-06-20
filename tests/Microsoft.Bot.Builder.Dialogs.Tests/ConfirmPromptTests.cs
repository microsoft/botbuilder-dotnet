// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class ConfirmPromptTests
    {
        [TestMethod]
        public async Task ConfirmPrompt()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ConfirmPrompt(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state, new PromptOptions { PromptString = "Please confirm." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    if (((ConfirmResult)dialogCompletion.Result).Confirmation)
                    {
                        await turnContext.SendActivity("Confirmed.");
                    }
                    else
                    {
                        await turnContext.SendActivity("Not confirmed.");
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm. (1) Yes or (2) No")
            .Send("yes")
            .AssertReply("Confirmed.")
            .StartTest();
        }

        [TestMethod]
        public async Task ConfirmPromptRetry()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ConfirmPrompt(Culture.English);

                var dialogCompletion = await prompt.Continue(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.Begin(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Please confirm.",
                            RetryPromptString = "Please confirm, say 'yes' or 'no' or something like that."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    if (((ConfirmResult)dialogCompletion.Result).Confirmation)
                    {
                        await turnContext.SendActivity("Confirmed.");
                    }
                    else
                    {
                        await turnContext.SendActivity("Not confirmed.");
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm. (1) Yes or (2) No")
            .Send("lala")
            .AssertReply("Please confirm, say 'yes' or 'no' or something like that. (1) Yes or (2) No")
            .Send("no")
            .AssertReply("Not confirmed.")
            .StartTest();
        }
    }
}
