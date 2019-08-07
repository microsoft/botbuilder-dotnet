// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Tests
{
    [TestClass]
    public class TelemetryWaterfallTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task Waterfall()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState));

            var telemetryClient = new Mock<IBotTelemetryClient>();
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);

            dialogs.Add(new WaterfallDialog("test", NewWaterfall()));
            dialogs.TelemetryClient = telemetryClient.Object;

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
            telemetryClient.Verify(m => m.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()), Times.Exactly(4));
            Console.WriteLine("Complete");
        }

        [TestMethod]
        public async Task WaterfallWithCallback()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var waterfallDialog = new WaterfallDialog("test", NewWaterfall());

            dialogs.Add(waterfallDialog);
            dialogs.TelemetryClient = telemetryClient.Object;

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
            telemetryClient.Verify(m => m.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()), Times.Exactly(4));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WaterfallWithActionsNull()
        {
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var waterfall = new WaterfallDialog("test") { TelemetryClient = telemetryClient.Object };
            waterfall.AddStep(null);
        }

        [TestMethod]
        public async Task EnsureEndDialogCalled()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var saved_properties = new Dictionary<string, IDictionary<string, string>>();
            var counter = 0;

            // Set up the client to save all logged property names and associated properties (in "saved_properties").
            telemetryClient.Setup(c => c.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()))
                            .Callback<string, IDictionary<string, string>, IDictionary<string, double>>((name, properties, metrics) => saved_properties.Add($"{name}_{counter++}", properties))
                            .Verifiable();
            var waterfallDialog = new MyWaterfallDialog("test", NewWaterfall());

            dialogs.Add(waterfallDialog);
            dialogs.TelemetryClient = telemetryClient.Object;

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
            .Send("hello")
            .AssertReply("step1")
            .StartTestAsync();
            telemetryClient.Verify(m => m.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()), Times.Exactly(7));

            // Verify:
            // Event name is "WaterfallComplete"
            // Event occurs on the 4th event logged
            // Event contains DialogId
            // Event DialogId is set correctly.
            Assert.IsTrue(saved_properties["WaterfallComplete_4"].ContainsKey("DialogId"));
            Assert.IsTrue(saved_properties["WaterfallComplete_4"]["DialogId"] == "test");
            Assert.IsTrue(saved_properties["WaterfallComplete_4"].ContainsKey("InstanceId"));
            Assert.IsTrue(saved_properties["WaterfallStep_1"].ContainsKey("InstanceId"));

            // Verify naming on lambda's is "StepXofY"
            Assert.IsTrue(saved_properties["WaterfallStep_1"].ContainsKey("StepName"));
            Assert.IsTrue(saved_properties["WaterfallStep_1"]["StepName"] == "Step1of3");
            Assert.IsTrue(saved_properties["WaterfallStep_1"].ContainsKey("InstanceId"));
            Assert.IsTrue(waterfallDialog.EndDialogCalled);
        }

        [TestMethod]
        public async Task EnsureCancelDialogCalled()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var telemetryClient = new Mock<IBotTelemetryClient>();
            var saved_properties = new Dictionary<string, IDictionary<string, string>>();
            var counter = 0;

            // Set up the client to save all logged property names and associated properties (in "saved_properties").
            telemetryClient.Setup(c => c.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()))
                            .Callback<string, IDictionary<string, string>, IDictionary<string, double>>((name, properties, metrics) => saved_properties.Add($"{name}_{counter++}", properties))
                            .Verifiable();

            var steps = new WaterfallStep[]
            {
                    async (step, cancellationToken) =>
                    {
                        await step.Context.SendActivityAsync("step1");
                        return Dialog.EndOfTurn;
                    },
                    async (step, cancellationToken) =>
                    {
                        await step.Context.SendActivityAsync("step2");
                        return Dialog.EndOfTurn;
                    },
                    async (step, cancellationToken) =>
                    {
                        await step.CancelAllDialogsAsync();
                        return Dialog.EndOfTurn;
                    },
            };
            var waterfallDialog = new MyWaterfallDialog("test", steps);

            dialogs.Add(waterfallDialog);
            dialogs.TelemetryClient = telemetryClient.Object;

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
            .AssertReply("step1")
            .StartTestAsync();
            telemetryClient.Verify(m => m.TrackEvent(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, double>>()), Times.Exactly(7));

            // Verify:
            // Event name is "WaterfallCancel"
            // Event occurs on the 4th event logged
            // Event contains DialogId
            // Event DialogId is set correctly.
            Assert.IsTrue(saved_properties["WaterfallStart_0"].ContainsKey("DialogId"));
            Assert.IsTrue(saved_properties["WaterfallStart_0"].ContainsKey("InstanceId"));
            Assert.IsTrue(saved_properties["WaterfallCancel_4"].ContainsKey("DialogId"));
            Assert.IsTrue(saved_properties["WaterfallCancel_4"]["DialogId"] == "test");
            Assert.IsTrue(saved_properties["WaterfallCancel_4"].ContainsKey("StepName"));
            Assert.IsTrue(saved_properties["WaterfallCancel_4"].ContainsKey("InstanceId"));

            // Event contains "StepName"
            // Event naming on lambda's is "StepXofY"
            Assert.IsTrue(saved_properties["WaterfallCancel_4"]["StepName"] == "Step3of3");
            Assert.IsTrue(waterfallDialog.CancelDialogCalled);
            Assert.IsFalse(waterfallDialog.EndDialogCalled);
        }

        private static WaterfallStep[] NewWaterfall()
        {
            return new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("step1");
                    return Dialog.EndOfTurn;
                },
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("step2");
                    return Dialog.EndOfTurn;
                },
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("step3");
                    return Dialog.EndOfTurn;
                },
            };
        }

        public class MyWaterfallDialog : WaterfallDialog
        {
            public MyWaterfallDialog(string id, IEnumerable<WaterfallStep> actions = null)
                : base(id, actions)
            {
            }

            public bool EndDialogCalled { get; set; } = false;

            public bool CancelDialogCalled { get; set; } = false;

            public override Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
            {
                if (reason == DialogReason.EndCalled)
                {
                    EndDialogCalled = true;
                }
                else if (reason == DialogReason.CancelCalled)
                {
                    CancelDialogCalled = true;
                }

                return base.EndDialogAsync(turnContext, instance, reason, cancellationToken);
            }
        }
    }
}
