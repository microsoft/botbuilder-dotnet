// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class DateTimePromptTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task BasicDateTimePrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var dateTimePrompt = new DateTimePrompt("DateTimePrompt", defaultLocale: Culture.English);
            dialogs.Add(dateTimePrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync();
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "What date would you like?" } };
                    await dc.PromptAsync("DateTimePrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var resolution = ((IList<DateTimeResolution>)results.Result).First();
                    var reply = MessageFactory.Text($"Timex:'{resolution.Timex}' Value:'{resolution.Value}'");
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("What date would you like?")
            .Send("5th December 2018 at 9am")
            .AssertReply("Timex:'2018-12-05T09' Value:'2018-12-05 09:00:00'")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task MultipleResolutionsDateTimePrompt()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var dateTimePrompt = new DateTimePrompt("DateTimePrompt", defaultLocale: Culture.English);
            dialogs.Add(dateTimePrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "What date would you like?" } };
                    await dc.PromptAsync("DateTimePrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var resolutions = (IList<DateTimeResolution>)results.Result;
                    var timexExpressions = resolutions.Select(r => r.Timex).Distinct();
                    var reply = MessageFactory.Text(string.Join(" ", timexExpressions));
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("What date would you like?")
            .Send("Wednesday 4 oclock")
            .AssertReply("XXXX-WXX-3T04 XXXX-WXX-3T16")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task DateTimePromptWithValidator()
        {
            string folder = Environment.CurrentDirectory;
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            TestAdapter adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            // Create new DialogSet.
            var dialogs = new DialogSet(dialogState);

            // Create and add number prompt to DialogSet.
            var dateTimePrompt = new DateTimePrompt("DateTimePrompt", CustomValidator, defaultLocale: Culture.English);
            dialogs.Add(dateTimePrompt);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    var options = new PromptOptions { Prompt = new Activity { Type = ActivityTypes.Message, Text = "What date would you like?" } };
                    await dc.PromptAsync("DateTimePrompt", options, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var resolution = ((IList<DateTimeResolution>)results.Result).First();
                    var reply = MessageFactory.Text($"Timex:'{resolution.Timex}' Value:'{resolution.Value}'");
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("What date would you like?")
            .Send("5th December 2018 at 9am")
            .AssertReply("Timex:'2018-12-05' Value:'2018-12-05'")
            .StartTestAsync();
        }

        private Task<bool> CustomValidator(PromptValidatorContext<IList<DateTimeResolution>> prompt, CancellationToken cancellationToken)
        {
            if (prompt.Recognized.Succeeded)
            {
                var resolution = prompt.Recognized.Value.First();

                // re-write the resolution to just include the date part.
                var rewrittenResolution = new DateTimeResolution
                {
                    Timex = resolution.Timex.Split('T')[0],
                    Value = resolution.Value.Split(' ')[0],
                };
                prompt.Recognized.Value = new List<DateTimeResolution> { rewrittenResolution };
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
