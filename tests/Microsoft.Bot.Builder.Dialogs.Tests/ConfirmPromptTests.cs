// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class ConfirmPromptTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConfirmPromptWithEmptyIdShouldFail()
        {
            var emptyId = "";
            var confirmPrompt = new ConfirmPrompt(emptyId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConfirmPromptWithNullIdShouldFail()
        {
            var nullId = "";
            nullId = null;
            var confirmPrompt = new ConfirmPrompt(nullId);
        }
        [TestMethod]
        public async Task ConfirmPrompt()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            // Create new DialogSet.
            DialogSet dialogs = new DialogSet(dialogState);
            dialogs.Add(new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    await dc.PromptAsync("ConfirmPrompt", new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Please confirm." } });
                }
                else if (!results.HasActive && results.HasResult)
                {
                    if ((bool)results.Result)
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
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            DialogSet dialogs = new DialogSet(dialogState);
            dialogs.Add(new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = "Please confirm."
                        },
                        RetryPrompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = "Please confirm, say 'yes' or 'no' or something like that."
                        }
                    };
                    await dc.PromptAsync("ConfirmPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    if ((bool)results.Result)
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
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            DialogSet dialogs = new DialogSet(dialogState);
            var prompt = new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English);
            // Set options
            prompt.ChoiceOptions = new Choices.ChoiceFactoryOptions { IncludeNumbers = true };
            dialogs.Add(prompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = "Please confirm."
                        },
                        RetryPrompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = "Please confirm, say 'yes' or 'no' or something like that."
                        }
                    };
                    await dc.PromptAsync("ConfirmPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    if ((bool)results.Result)
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
        public async Task ConfirmPromptChoiceOptionsNoNumbers()
        {
            ConversationState convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter()
                .Use(convoState);

            DialogSet dialogs = new DialogSet(dialogState);
            var prompt = new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English);
            // Set options
            prompt.ChoiceOptions = new Choices.ChoiceFactoryOptions { IncludeNumbers = false, InlineSeparator = "~" };
            dialogs.Add(prompt);


            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);

                var results = await dc.ContinueAsync();
                if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = "Please confirm."
                        },
                        RetryPrompt = new Activity
                        {
                            Type = ActivityTypes.Message,
                            Text = "Please confirm, say 'yes' or 'no' or something like that."
                        }
                    };
                    await dc.PromptAsync("ConfirmPrompt", options);
                }
                else if (!results.HasActive && results.HasResult)
                {
                    if ((bool)results.Result)
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
