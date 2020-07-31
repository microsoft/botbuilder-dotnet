// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Castle.Core.Internal;
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
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

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
        public async Task TextPromptWithNaughtyStrings()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            var dialogs = new DialogSet(dialogState);

            var textPrompt = new TextPrompt("TextPrompt");
            dialogs.Add(textPrompt);

            var filePath = Path.Combine(new string[] { "..", "..", "..", "Resources", "naughtyStrings.txt" });
            using var sr = new StreamReader(filePath);
            var naughtyString = string.Empty;
            do
            {
                naughtyString = GetNextNaughtyString(sr);
                try 
                {
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
                            await turnContext.SendActivityAsync(MessageFactory.Text(textResult), cancellationToken);
                        }
                    })
                    .Send("hello")
                    .AssertReply("Enter some text.")
                    .Send(naughtyString)
                    .AssertReply(naughtyString)
                    .StartTestAsync();
                }
                catch (Exception e)
                {
                    // If the input message is empty after a .Trim() operation, character the comparison will fail because the reply message will be a
                    // Message Activity with null as Text, this is expected behavior
                    var messageIsBlank = e.Message.Equals(" :\nExpected: \nReceived:", StringComparison.CurrentCultureIgnoreCase) && naughtyString.Equals(" ", StringComparison.CurrentCultureIgnoreCase);
                    var messageIsEmpty = e.Message.Equals(":\nExpected:\nReceived:", StringComparison.CurrentCultureIgnoreCase) && naughtyString.IsNullOrEmpty();
                    if (!(messageIsBlank || messageIsEmpty))
                    {
                        throw;
                    }
                }
            }
            while (!string.IsNullOrEmpty(naughtyString));
        }

        [TestMethod]
        public async Task TextPromptValidator()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

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
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

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
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

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

        private static string GetNextNaughtyString(StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                {
                    // do nothing. Read next line. 
                }
                else
                {
                    return line;
                }
            }

            return string.Empty;
        }
    }
}
