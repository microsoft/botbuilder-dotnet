// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class PromptValidatorContextTests
    {
        [TestMethod]
        public async Task PromptValidatorContextEnd()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            dialogs.Add(new TextPrompt("namePrompt", (promptContext, cancellationToken) => Task.FromResult(true)));

            var steps = new WaterfallStep[]
            {
                async (stepContext, cancellationToken) =>
                {
                    return await stepContext.PromptAsync("namePrompt", new PromptOptions { Prompt = new Activity { Text = "Please type your name.", Type = ActivityTypes.Message } }, cancellationToken);
                },
                async (stepContext, cancellationToken) =>
                {
                    var name = (string)stepContext.Result;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{name} is a great name!"), cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                },
            };
            dialogs.Add(new WaterfallDialog(
                "nameDialog",
                steps));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                    await dc.ContinueDialogAsync(cancellationToken);
                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync("nameDialog", cancellationToken: cancellationToken);
                    }
                })
                .Send("hello")
                .AssertReply("Please type your name.")
                .Send("John")
                .AssertReply("John is a great name!")
                .Send("Hi again")
                .AssertReply("Please type your name.")
                .Send("1")
                .AssertReply("1 is a great name!")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task PromptValidatorContextRetryEnd()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            // Create TextPrompt with dialogId "namePrompt" and custom validator
            dialogs.Add(new TextPrompt("namePrompt", async (promptContext, cancellationToken) =>
            {
                var result = promptContext.Recognized.Value;
                if (result.Length > 3)
                {
                    return true;
                }
                else
                {
                    await promptContext.Context.SendActivityAsync(MessageFactory.Text("Please send a name that is longer than 3 characters."), cancellationToken);
                }

                return false;
            }));

            var steps = new WaterfallStep[]
            {
                async (stepContext, cancellationToken) =>
                {
                    return await stepContext.PromptAsync("namePrompt", new PromptOptions { Prompt = new Activity { Text = "Please type your name.", Type = ActivityTypes.Message } }, cancellationToken);
                },
                async (stepContext, cancellationToken) =>
                {
                    var name = (string)stepContext.Result;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{name} is a great name!"), cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                },
            };
            dialogs.Add(new WaterfallDialog(
                "nameDialog",
                steps));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                    await dc.ContinueDialogAsync(cancellationToken);
                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync("nameDialog", null, cancellationToken);
                    }
                })
                .Send("hello")
                .AssertReply("Please type your name.")
                .Send("hi")
                .AssertReply("Please send a name that is longer than 3 characters.")
                .Send("John")
                .AssertReply("John is a great name!")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task PromptValidatorNumberOfAttempts()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            // Create TextPrompt with dialogId "namePrompt" and custom validator
            dialogs.Add(new TextPrompt("namePrompt", async (promptContext, cancellationToken) =>
            {
                var result = promptContext.Recognized.Value;
                if (result.Length > 3)
                {
                    var succeededMessage = MessageFactory.Text($"You got it at the {promptContext.AttemptCount}th try!");
                    await promptContext.Context.SendActivityAsync(succeededMessage, cancellationToken);
                    return true;
                }

                var reply = MessageFactory.Text($"Please send a name that is longer than 3 characters. {promptContext.AttemptCount}");
                await promptContext.Context.SendActivityAsync(reply, cancellationToken);

                return false;
            }));

            var steps = new WaterfallStep[]
            {
                async (stepContext, cancellationToken) =>
                {
                    return await stepContext.PromptAsync("namePrompt", new PromptOptions { Prompt = new Activity { Text = "Please type your name.", Type = ActivityTypes.Message } }, cancellationToken);
                },
                async (stepContext, cancellationToken) =>
                {
                    var name = (string)stepContext.Result;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{name} is a great name!"), cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                },
            };
            dialogs.Add(new WaterfallDialog(
                "nameDialog",
                steps));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                    await dc.ContinueDialogAsync(cancellationToken);
                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync("nameDialog", null, cancellationToken);
                    }
                })
                .Send("hello")
                .AssertReply("Please type your name.")
                .Send("hi")
                .AssertReply("Please send a name that is longer than 3 characters. 1")
                .Send("hi")
                .AssertReply("Please send a name that is longer than 3 characters. 2")
                .Send("hi")
                .AssertReply("Please send a name that is longer than 3 characters. 3")
                .Send("John")
                .AssertReply("You got it at the 4th try!")
                .AssertReply("John is a great name!")
                .StartTestAsync();
        }
    }
}
