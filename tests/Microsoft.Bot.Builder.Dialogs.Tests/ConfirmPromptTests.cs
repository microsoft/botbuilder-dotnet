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

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state, new PromptOptions { PromptString = "Please confirm." });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    if (((ConfirmResult)dialogCompletion.Result).Confirmation)
                    {
                        await turnContext.SendActivityAsync("Confirmed.");
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("Not confirmed.");
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm. (1) Yes or (2) No")
            .Send("yes")
            .AssertReply("Confirmed.")
            .StartTestAsync();
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

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state,
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
                        await turnContext.SendActivityAsync("Confirmed.");
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("Not confirmed.");
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm. (1) Yes or (2) No")
            .Send("lala")
            .AssertReply("Please confirm, say 'yes' or 'no' or something like that. (1) Yes or (2) No")
            .Send("no")
            .AssertReply("Not confirmed.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ConfirmPromptChoiceOptionsNumbers()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ConfirmPrompt(Culture.English);

                // Set options
                Choices.ChoiceFactoryOptions options = new Choices.ChoiceFactoryOptions();
                options.IncludeNumbers = true;
                prompt.ChoiceOptions = options;

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state,
                        new PromptOptions
                        {
                            PromptString = "Please confirm.",
                            RetryPromptString = "Please confirm, say 'Yes' or 'No' or something like that."
                        });
                }
                else if (dialogCompletion.IsCompleted)
                {
                    if (((ConfirmResult)dialogCompletion.Result).Confirmation)
                    {
                        await turnContext.SendActivityAsync("Confirmed.");
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("Not confirmed.");
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm. (1) Yes or (2) No")
            .Send("lala")
            .AssertReply("Please confirm, say 'Yes' or 'No' or something like that. (1) Yes or (2) No")
            .Send("no")
            .AssertReply("Not confirmed.")
            .StartTestAsync();
        }
        [TestMethod]
        public async Task ConfirmPromptChoiceOptionsNoNumbers()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

            await new TestFlow(adapter, async (turnContext) =>
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var prompt = new ConfirmPrompt(Culture.English);

                // Set options
                Choices.ChoiceFactoryOptions options = new Choices.ChoiceFactoryOptions();
                options.IncludeNumbers = false;
                options.InlineSeparator = "~"; // Doesn't make sense for ConfirmPrompt =
                prompt.ChoiceOptions = options;

                var dialogCompletion = await prompt.ContinueAsync(turnContext, state);
                if (!dialogCompletion.IsActive && !dialogCompletion.IsCompleted)
                {
                    await prompt.BeginAsync(turnContext, state,
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
                        await turnContext.SendActivityAsync("Confirmed.");
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("Not confirmed.");
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm. Yes or No")
            .Send("lala")
            .AssertReply("Please confirm, say 'yes' or 'no' or something like that. Yes or No")
            .Send("no")
            .AssertReply("Not confirmed.")
            .StartTestAsync();
        }
    }
}

