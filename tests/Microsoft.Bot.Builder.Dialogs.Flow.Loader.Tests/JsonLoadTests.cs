using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Builder.Dialogs.Flow;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Converters;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Contract;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Loader.Tests
{
    [TestClass]
    public class JsonLoadTests
    {
        [TestMethod]
        public async Task JsonDialogLoad_ChainedPromptsAndSelfLoop()
        {
            string json = File.ReadAllText("TestFlows/ChainedPromptsAndSwitch.json");

            var dialog = DialogLoader.Load(json);

            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger()))
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            dialogs.Add(dialog);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());

                var dialogContext = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                    results = await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken);
            })
            .Send("hello")
            //.AssertReply("What is your name?")
            //.Send("x")
            .AssertReply("What is your name?")
            .Send("Joe")
            .AssertReply("What is your age?")
            .Send("64")
            .AssertReply("Done")
            .StartTestAsync();

        }
    }
}
