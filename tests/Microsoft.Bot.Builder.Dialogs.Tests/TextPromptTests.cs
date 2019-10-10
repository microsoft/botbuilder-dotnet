// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class TextPromptTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TextPromptWithEmptyIdShouldFail()
        {
            var emptyId = string.Empty;
            var textPrompt = new TextPrompt(emptyId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TextPromptWithNullIdShouldFail()
        {
            var nullId = string.Empty;
            nullId = null;
            var textPrompt = new TextPrompt(nullId);
        }

        [TestMethod]
        public async Task TextPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var dialogs = new DialogSet(dialogState);

            var textPrompt = new TextPrompt("TextPrompt");
            dialogs.Add(textPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter some text." } };
                    await dc.PromptAsync("TextPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var textResult = (string)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the text '{textResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("some text")
            .AssertReply("Bot received the text 'some text'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TextPromptValidator()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var dialogs = new DialogSet(dialogState);

            PromptValidator<string> validator = async (promptContext, cancellationToken) =>
            {
                var value = promptContext.Recognized.Value;
                if (value.Length <= 3)
                {
                    await promptContext.Context.SendActivityAsync(MessageFactory.Text("Make sure the text is greater than three characters."), cancellationToken);
                    return false;
                }
                else
                {
                    return true;
                }
            };

            var textPrompt = new TextPrompt("TextPrompt", validator);
            dialogs.Add(textPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter some text." } };
                    await dc.PromptAsync("TextPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var textResult = (string)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the text '{textResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("hi")
            .AssertReply("Make sure the text is greater than three characters.")
            .Send("hello")
            .AssertReply("Bot received the text 'hello'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TextPromptWithRetryPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var dialogs = new DialogSet(dialogState);

            PromptValidator<string> validator = (promptContext, cancellationToken) =>
            {
                var value = promptContext.Recognized.Value;
                if (value.Length >= 3)
                {
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            };
            var textPrompt = new TextPrompt("TextPrompt", validator);
            dialogs.Add(textPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter some text." },
                        RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = "Make sure the text is greater than three characters." },
                    };
                    await dc.PromptAsync("TextPrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var textResult = (string)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the text '{textResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("hi")
            .AssertReply("Make sure the text is greater than three characters.")
            .Send("hello")
            .AssertReply("Bot received the text 'hello'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TextPromptValidatorWithMessageShouldNotSendRetryPrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var dialogs = new DialogSet(dialogState);

            PromptValidator<string> validator = async (promptContext, cancellationToken) =>
            {
                var value = promptContext.Recognized.Value;
                if (value.Length <= 3)
                {
                    await promptContext.Context.SendActivityAsync(MessageFactory.Text("The text should be greater than 3 chars."), cancellationToken);
                    return false;
                }
                else
                {
                    return true;
                }
            };
            var textPrompt = new TextPrompt("TextPrompt", validator);
            dialogs.Add(textPrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions
                    {
                        Prompt = new Activity { Type = ActivityTypes.Message, Text = "Enter some text." },
                        RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = "Make sure the text is greater than three characters." },
                    };
                    await dc.PromptAsync("TextPrompt", options);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var textResult = (string)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the text '{textResult}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter some text.")
            .Send("hi")
            .AssertReply("The text should be greater than 3 chars.")
            .Send("hello")
            .AssertReply("Bot received the text 'hello'.")
            .StartTestAsync();
        }
    }
}
