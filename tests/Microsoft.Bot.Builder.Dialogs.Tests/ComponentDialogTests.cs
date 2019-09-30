// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("ComponentDialog Tests")]
    public class ComponentDialogTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task CallDialogInParentComponent()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(dialogState);

                var childComponent = new ComponentDialog("childComponent");
                var childStep = new WaterfallStep[]
                    {
                        async (step, token) =>
                        {
                            await step.Context.SendActivityAsync("Child started.");
                            return await step.BeginDialogAsync("parentDialog", "test");
                        },
                        async (step, token) =>
                        {
                            await step.Context.SendActivityAsync($"Child finished. Value: {step.Result}");
                            return await step.EndDialogAsync();
                        }
                    };
                childComponent.AddDialog(new WaterfallDialog("childDialog", childStep));

                var parentComponent = new ComponentDialog("parentComponent");
                parentComponent.AddDialog(childComponent);
                var parentStep = new WaterfallStep[]
                    {
                        async (step, token) =>
                        {
                            await step.Context.SendActivityAsync("Parent called.");
                            return await step.EndDialogAsync(step.Options);
                        }
                    };
                parentComponent.AddDialog(new WaterfallDialog("parentDialog", parentStep));

                dialogs.Add(parentComponent);

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync("parentComponent", null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{value}'."), cancellationToken);
                }
            })
            .Send("Hi")
                .AssertReply("Child started.")
                .AssertReply("Parent called.")
                .AssertReply("Child finished. Value: test")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task BasicWaterfallTest()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(CreateWaterfall());
                dialogs.Add(new NumberPrompt<int>("number", defaultLocale: Culture.English));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync("test-waterfall", null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{value}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Thanks for '42'")
            .AssertReply("Enter another number.")
            .Send("64")
            .AssertReply("Bot received the number '64'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task TelemetryBasicWaterfallTest()
        {
            var testComponentDialog = new TestComponentDialog();
            Assert.IsTrue(testComponentDialog.TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("test-waterfall").TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("number").TelemetryClient is NullBotTelemetryClient);

            testComponentDialog.TelemetryClient = new MyBotTelemetryClient();
            Assert.IsTrue(testComponentDialog.TelemetryClient is MyBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("test-waterfall").TelemetryClient is MyBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("number").TelemetryClient is MyBotTelemetryClient);
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task TelemetryHeterogeneousLoggerTest()
        {
            var testComponentDialog = new TestComponentDialog();
            Assert.IsTrue(testComponentDialog.TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("test-waterfall").TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("number").TelemetryClient is NullBotTelemetryClient);

            testComponentDialog.FindDialog("test-waterfall").TelemetryClient = new MyBotTelemetryClient();

            Assert.IsTrue(testComponentDialog.FindDialog("test-waterfall").TelemetryClient is MyBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("number").TelemetryClient is NullBotTelemetryClient);
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task TelemetryAddWaterfallTest()
        {
            var testComponentDialog = new TestComponentDialog();
            Assert.IsTrue(testComponentDialog.TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("test-waterfall").TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("number").TelemetryClient is NullBotTelemetryClient);

            testComponentDialog.TelemetryClient = new MyBotTelemetryClient();
            testComponentDialog.AddDialog(new WaterfallDialog("C"));

            Assert.IsTrue(testComponentDialog.FindDialog("C").TelemetryClient is MyBotTelemetryClient);
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task TelemetryNullUpdateAfterAddTest()
        {
            var testComponentDialog = new TestComponentDialog();
            Assert.IsTrue(testComponentDialog.TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("test-waterfall").TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("number").TelemetryClient is NullBotTelemetryClient);

            testComponentDialog.TelemetryClient = new MyBotTelemetryClient();
            testComponentDialog.AddDialog(new WaterfallDialog("C"));

            Assert.IsTrue(testComponentDialog.FindDialog("C").TelemetryClient is MyBotTelemetryClient);
            testComponentDialog.TelemetryClient = null;

            Assert.IsTrue(testComponentDialog.FindDialog("test-waterfall").TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("number").TelemetryClient is NullBotTelemetryClient);
            Assert.IsTrue(testComponentDialog.FindDialog("C").TelemetryClient is NullBotTelemetryClient);

            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task BasicComponentDialogTest()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(new TestComponentDialog());

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync("TestComponentDialog", null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{value}'."), cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Enter a number.")
            .Send("42")
            .AssertReply("Thanks for '42'")
            .AssertReply("Enter another number.")
            .Send("64")
            .AssertReply("Bot received the number '64'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task NestedComponentDialogTest()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(new TestNestedComponentDialog());

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync("TestNestedComponentDialog", null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{value}'."), cancellationToken);
                }
            })
            .Send("hello")

            // step 1
            .AssertReply("Enter a number.")

            // step 2
            .Send("42")
            .AssertReply("Thanks for '42'")
            .AssertReply("Enter another number.")

            // step 3 and step 1 again (nested component)
            .Send("64")
            .AssertReply("Got '64'.")
            .AssertReply("Enter a number.")

            // step 2 again (from the nested component)
            .Send("101")
            .AssertReply("Thanks for '101'")
            .AssertReply("Enter another number.")

            // driver code
            .Send("5")
            .AssertReply("Bot received the number '5'.")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task CallDialogDefinedInParentComponent()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var options = new Dictionary<string, string> { { "value", "test" } };

            var childComponent = new ComponentDialog("childComponent");
            var childActions = new WaterfallStep[]
            {
                async (step, ct) =>
                {
                    await step.Context.SendActivityAsync("Child started.");
                    return await step.BeginDialogAsync("parentDialog", options);
                },
                async (step, ct) =>
                {
                    Assert.AreEqual("test", (string)step.Result);
                    await step.Context.SendActivityAsync("Child finished.");
                    return await step.EndDialogAsync();
                },
            };
            childComponent.AddDialog(new WaterfallDialog(
                "childDialog",
                childActions));

            var parentComponent = new ComponentDialog("parentComponent");
            parentComponent.AddDialog(childComponent);
            var parentActions = new WaterfallStep[]
            {
                async (step, dc) =>
                {
                    var stepOptions = step.Options as IDictionary<string, string>;
                    Assert.IsNotNull(stepOptions);
                    Assert.IsTrue(stepOptions.ContainsKey("value"));
                    await step.Context.SendActivityAsync($"Parent called with: {stepOptions["value"]}");
                    return await step.EndDialogAsync(stepOptions["value"]);
                },
            };
            parentComponent.AddDialog(new WaterfallDialog(
                "parentDialog",
                parentActions));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dialogs = new DialogSet(dialogState);
                dialogs.Add(parentComponent);

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync("parentComponent", null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text("Done"), cancellationToken);
                }
            })
            .Send("Hi")
            .AssertReply("Child started.")
            .AssertReply("Parent called with: test")
            .AssertReply("Child finished.")
            .StartTestAsync();
        }

        private static TestFlow CreateTestFlow(WaterfallDialog waterfallDialog)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(convoState));

            var testFlow = new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState(), cancellationToken);
                var dialogs = new DialogSet(dialogState);

                dialogs.Add(new CancelledComponentDialog(waterfallDialog));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    results = await dc.BeginDialogAsync("TestComponentDialog", null, cancellationToken);
                }

                if (results.Status == DialogTurnStatus.Cancelled)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Component dialog cancelled (result value is {results.Result?.ToString()})."), cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    var value = (int)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bot received the number '{value}'."), cancellationToken);
                }
            });
            return testFlow;
        }

        private static WaterfallDialog CreateWaterfall()
        {
            var steps = new WaterfallStep[]
            {
                WaterfallStep1,
                WaterfallStep2,
            };
            return new WaterfallDialog("test-waterfall", steps);
        }

        private static async Task<DialogTurnResult> WaterfallStep1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter a number.") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> WaterfallStep2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks for '{numberResult}'"), cancellationToken);
            }

            return await stepContext.PromptAsync("number", new PromptOptions { Prompt = MessageFactory.Text("Enter another number.") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> WaterfallStep3(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Got '{numberResult}'."), cancellationToken);
            }

            return await stepContext.BeginDialogAsync("TestComponentDialog", null, cancellationToken);
        }

        private static Task<DialogTurnResult> CancelledWaterfallStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Cancelled, 42));
        }

        private class TestComponentDialog : ComponentDialog
        {
            public TestComponentDialog()
                : base("TestComponentDialog")
            {
                AddDialog(CreateWaterfall());
                AddDialog(new NumberPrompt<int>("number", defaultLocale: Culture.English));
            }
        }

        private class TestNestedComponentDialog : ComponentDialog
        {
            public TestNestedComponentDialog()
                : base("TestNestedComponentDialog")
            {
                var steps = new WaterfallStep[]
                {
                    WaterfallStep1,
                    WaterfallStep2,
                    WaterfallStep3,
                };
                AddDialog(new WaterfallDialog(
                    "test-waterfall",
                    steps));
                AddDialog(new NumberPrompt<int>("number", defaultLocale: Culture.English));
                AddDialog(new TestComponentDialog());
            }
        }

        private class CancelledComponentDialog : ComponentDialog
        {
            public CancelledComponentDialog(Dialog waterfallDialog)
                : base("TestComponentDialog")
            {
                AddDialog(waterfallDialog);
                AddDialog(new NumberPrompt<int>("number", defaultLocale: Culture.English));
            }
        }

        private class MyBotTelemetryClient : IBotTelemetryClient
        {
            public MyBotTelemetryClient()
            {
            }

            public void Flush()
            {
                throw new NotImplementedException();
            }

            public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string message = null, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
            {
                throw new NotImplementedException();
            }

            public void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
            {
                throw new NotImplementedException();
            }

            public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
            {
                throw new NotImplementedException();
            }

            public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
            {
                throw new NotImplementedException();
            }

            public void TrackTrace(string message, Severity severityLevel, IDictionary<string, string> properties)
            {
                throw new NotImplementedException();
            }
        }
    }
}
