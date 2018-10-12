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
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.PromptAsync("ConfirmPrompt", new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Please confirm." } }, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if ((bool)results.Result)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Confirmed."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Not confirmed."), cancellationToken);
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
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
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
                    await dc.PromptAsync("ConfirmPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if ((bool)results.Result)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Confirmed."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Not confirmed."), cancellationToken);
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
        public async Task ConfirmPromptNoOptions()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions();
                    await dc.PromptAsync("ConfirmPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if ((bool)results.Result)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Confirmed."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Not confirmed."), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply(" (1) Yes or (2) No")
            .Send("lala")
            .AssertReply(" (1) Yes or (2) No")
            .Send("no")
            .AssertReply("Not confirmed.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ConfirmPromptChoiceOptionsNumbers()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);
            var prompt = new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English);
            // Set options
            prompt.ChoiceOptions = new Choices.ChoiceFactoryOptions { IncludeNumbers = true };
            prompt.Style = Choices.ListStyle.Inline;
            dialogs.Add(prompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
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
                    await dc.PromptAsync("ConfirmPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if ((bool)results.Result)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Confirmed."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Not confirmed."), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm. (1) Yes or (2) No")
            .Send("lala")
            .AssertReply("Please confirm, say 'yes' or 'no' or something like that. (1) Yes or (2) No")
            .Send("2")
            .AssertReply("Not confirmed.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ConfirmPromptChoiceOptionsMultipleAttempts()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);
            var prompt = new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English);
            // Set options
            prompt.ChoiceOptions = new Choices.ChoiceFactoryOptions { IncludeNumbers = true };
            prompt.Style = Choices.ListStyle.Inline;
            dialogs.Add(prompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
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
                    await dc.PromptAsync("ConfirmPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if ((bool)results.Result)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Confirmed."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Not confirmed."), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm. (1) Yes or (2) No")
            .Send("lala")
            .AssertReply("Please confirm, say 'yes' or 'no' or something like that. (1) Yes or (2) No")
            .Send("what")
            .AssertReply("Please confirm, say 'yes' or 'no' or something like that. (1) Yes or (2) No")
            .Send("2")
            .AssertReply("Not confirmed.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task ConfirmPromptChoiceOptionsNoNumbers()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);
            var prompt = new ConfirmPrompt("ConfirmPrompt", defaultLocale: Culture.English);
            // Set options
            prompt.ChoiceOptions = new Choices.ChoiceFactoryOptions { IncludeNumbers = false, InlineSeparator = "~" };
            dialogs.Add(prompt);


            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
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
                    await dc.PromptAsync("ConfirmPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    if ((bool)results.Result)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Confirmed."), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Not confirmed."), cancellationToken);
                    }
                }
            })
            .Send("hello")
            .AssertReply("Please confirm. Yes or No")
            .Send("2")
            .AssertReply("Please confirm, say 'yes' or 'no' or something like that. Yes or No")
            .Send("no")
            .AssertReply("Not confirmed.")
            .StartTestAsync();
        }
    }
}
