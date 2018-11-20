// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.ApplicationInsights.Tests
{
    [TestClass]
    public class TelemetryWaterfallTests
    {

        [TestMethod]
        public async Task Waterfall()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var telemetryClient = new Mock<IBotTelemetryClient>();
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
            {
                async (step, cancellationToken) => { await step.Context.SendActivityAsync("step1"); return Dialog.EndOfTurn; },
                async (step, cancellationToken) => { await step.Context.SendActivityAsync("step2"); return Dialog.EndOfTurn; },
                async (step, cancellationToken) => { await step.Context.SendActivityAsync("step3"); return Dialog.EndOfTurn; },
            })
            { Logger = telemetryClient.Object });

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .Send("hello")
            .AssertReply("step2")
            .Send("hello")
            .AssertReply("step3")
            .StartTestAsync();
            telemetryClient.Verify(m => m.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string,string>>(), It.IsAny<IDictionary<string, double>>()), Times.Exactly(3));
            Console.WriteLine("Complete");
        }

        [TestMethod]
        public async Task WaterfallWithCallback()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var telemetryClient = new Mock<IBotTelemetryClient>(); ;
            var waterfallDialog = new WaterfallDialog("test", new WaterfallStep[]
            {
                    async (step, cancellationToken) => { await step.Context.SendActivityAsync("step1"); return Dialog.EndOfTurn; },
                    async (step, cancellationToken) => { await step.Context.SendActivityAsync("step2"); return Dialog.EndOfTurn; },
                    async (step, cancellationToken) => { await step.Context.SendActivityAsync("step3"); return Dialog.EndOfTurn; },
            })
            { Logger = telemetryClient.Object };

            dialogs.Add(waterfallDialog);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .Send("hello")
            .AssertReply("step2")
            .Send("hello")
            .AssertReply("step3")
            .StartTestAsync();
            telemetryClient.Verify(m => m.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()), Times.Exactly(3));
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WaterfallWithStepsNull()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var waterfall = new WaterfallDialog("test") { Logger = telemetryClient.Object };
            waterfall.AddStep(null);
        }

    }
}
