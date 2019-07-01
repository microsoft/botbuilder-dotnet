// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class BindingTestDialog : Dialog
    {
        public BindingTestDialog(string dialogId, string inputBinding, string outputBinding)
            : base(dialogId)
        {
            this.InputBindings[DialogContextState.DIALOG_VALUE] = inputBinding;
            this.OutputBinding = outputBinding;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var bindingValue = dc.State.GetValue<string>(DialogContextState.DIALOG_VALUE);
            return await dc.EndDialogAsync(bindingValue).ConfigureAwait(false);
        }
    }

    [TestClass]
    public class WaterfallTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task Waterfall()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
            {
                async (step, cancellationToken) => { await step.Context.SendActivityAsync("step1"); return Dialog.EndOfTurn; },
                async (step, cancellationToken) => { await step.Context.SendActivityAsync("step2"); return Dialog.EndOfTurn; },
                async (step, cancellationToken) => { await step.Context.SendActivityAsync("step3"); return Dialog.EndOfTurn; },
            }));

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
        }

        [TestMethod]
        public async Task Waterfall_PersistsMemory()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);

            dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await Task.Delay(0);
                    step.State.User["name"] = "bill";
                    step.State.Conversation["order"] = 1;
                    step.State.Dialog["result"] = "foo";
                    return Dialog.EndOfTurn;
                },
                async (step, cancellationToken) =>
                {
                    await Task.Delay(0);
                    Assert.AreEqual("bill", step.State.User["name"]);
                    Assert.AreEqual(1, step.State.Conversation["order"]);
                    Assert.AreEqual("foo", step.State.Dialog["result"]);
                    return Dialog.EndOfTurn; },
            }));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("start")
            .Send("continue")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Waterfall_MemoryQueryStateValues()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);

            dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await Task.Delay(0);
                    step.State.User["name"] = "user";
                    step.State.Conversation["name"] = "convo";
                    step.State.Dialog["name"] = "foo";
                    return Dialog.EndOfTurn;
                },
                async (step, cancellationToken) =>
                {
                    await Task.Delay(0);
                    var result = step.State.Query("user.name");
                    Assert.IsTrue(result.Count() == 1 && result.First().ToString() == "user");

                    result = step.State.Query("conversation.name");
                    Assert.IsTrue(result.Count() == 1 && result.First().ToString() == "convo");

                    result = step.State.Query("dialog.name");
                    Assert.IsTrue(result.Count() == 1 && result.First().ToString() == "foo");

                    result = step.State.Query("$..name");
                    Assert.AreEqual(6, result.Count(), "jpath query returns wrong count");

                    return Dialog.EndOfTurn;
                }
            }));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("start")
            .Send("continue")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Waterfall_MemoryGetStateValues()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);

            dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await Task.Delay(0);
                    step.State.User["name"] = "user";
                    step.State.Conversation["name"] = "convo";
                    step.State.Dialog["name"] = "foo";
                    return Dialog.EndOfTurn;
                },
                async (step, cancellationToken) =>
                {
                    await Task.Delay(0);
                    Assert.AreEqual("user", step.State.GetValue<string>("user.name"));
                    Assert.AreEqual(false, step.State.HasValue("user.lastName"));
                    Assert.AreEqual("default", step.State.GetValue<string>("user.lastName", "default"));
                    return Dialog.EndOfTurn;
                }
            }));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("start")
            .Send("continue")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Waterfall_MemorySetValue()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);

            dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await Task.Delay(0);
                    step.State.User["name"] = "testUser";
                    step.State.SetValue("user.address", "15155");
                    step.State.SetValue("user.profile.firstName", "bill");
                    step.State.SetValue("conversation.a", "joe");
                    step.State.SetValue("dialog.profilds", "johnny");
                    step.State.SetValue("conversation.profile.firstName", "joe");
                    step.State.SetValue("dialog.profile.firstName", "johnny");
                    return Dialog.EndOfTurn;
                },
                async (step, cancellationToken) =>
                {
                    await Task.Delay(0);
                    Assert.AreEqual("bill", step.State.GetValue<string>("user.profile.firstName"));
                    Assert.AreEqual("joe", step.State.GetValue<string>("conversation.profile.firstName"));
                    Assert.AreEqual("johnny", step.State.GetValue<string>("dialog.profile.firstName"));
                    return Dialog.EndOfTurn;
                }
            }));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("start")
            .Send("continue")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task Waterfall_MemoryInputOutputBindings()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState, userState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var userStateProperty = userState.CreateProperty<Dictionary<string, object>>("user");
            var convoStateProperty = convoState.CreateProperty<Dictionary<string, object>>("conversation");

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);

            dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    step.State.SetValue("user.profile.name", "bill");
                    await step.BeginDialogAsync("b").ConfigureAwait(false);
                    return Dialog.EndOfTurn;
                },
                async (step, cancellationToken) =>
                {
                    await Task.Delay(0);
                    Assert.AreEqual("bill", step.State.GetValue<string>("dialog.name"));
                    return Dialog.EndOfTurn;
                }
            }));

            dialogs.Add(new BindingTestDialog("b", "user.profile.name", "dialog.name"));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("start")
            .StartTestAsync();
        }


        [TestMethod]
        public async Task WaterfallWithCallback()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var waterfallDialog = new WaterfallDialog("test", new WaterfallStep[]
            {
                async (step, cancellationToken) => { await step.Context.SendActivityAsync("step1"); return Dialog.EndOfTurn; },
                async (step, cancellationToken) => { await step.Context.SendActivityAsync("step2"); return Dialog.EndOfTurn; },
                async (step, cancellationToken) => { await step.Context.SendActivityAsync("step3"); return Dialog.EndOfTurn; },
            });

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
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WaterfallWithStepsNull()
        {
            var waterfall = new WaterfallDialog("test");
            waterfall.AddStep(null);
        }

        [TestMethod]
        public async Task WaterfallWithClass()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new MyWaterfallDialog("test"));

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
        }


        [TestMethod]
        public async Task WaterfallPrompt()
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
                dialogs.Add(Create_Waterfall2());
                var numberPrompt = new NumberPrompt<int>("number", defaultLocale: Culture.English);
                dialogs.Add(numberPrompt);

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                await dc.ContinueDialogAsync();

                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test-waterfall");
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .AssertReply("Enter a number.")
            .Send("hello again")
            .AssertReply("It must be a number")
            .Send("42")
            .AssertReply("Thanks for '42'")
            .AssertReply("step2")
            .AssertReply("Enter a number.")
            .Send("apple")
            .AssertReply("It must be a number")
            .Send("orange")
            .AssertReply("It must be a number")
            .Send("64")
            .AssertReply("Thanks for '64'")
            .AssertReply("step3")
            .StartTestAsync();
        }

        private static WaterfallDialog Create_Waterfall2()
        {
            return new WaterfallDialog("test-waterfall", new WaterfallStep[] {
                Waterfall2_Step1,
                Waterfall2_Step2,
                Waterfall2_Step3
            });
        }

        private static async Task<DialogTurnResult> Waterfall2_Step1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("step1");
            return await stepContext.PromptAsync("number", new PromptOptions
            {
                Prompt = MessageFactory.Text("Enter a number."),
                RetryPrompt = MessageFactory.Text("It must be a number")
            });
        }
        private static async Task<DialogTurnResult> Waterfall2_Step2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await stepContext.Context.SendActivityAsync($"Thanks for '{numberResult}'");
            }
            await stepContext.Context.SendActivityAsync("step2");
            return await stepContext.PromptAsync("number",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter a number."),
                    RetryPrompt = MessageFactory.Text("It must be a number")
                });
        }
        private static async Task<DialogTurnResult> Waterfall2_Step3(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Values != null)
            {
                var numberResult = (int)stepContext.Result;
                await stepContext.Context.SendActivityAsync($"Thanks for '{numberResult}'");
            }
            await stepContext.Context.SendActivityAsync("step3");
            return await stepContext.EndDialogAsync(new Dictionary<string, object> { { "Value", "All Done!" } });
        }

        [TestMethod]
        public async Task WaterfallNested()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dialogState = convoState.CreateProperty<DialogState>("dialogState");
                var dialogs = new DialogSet(dialogState);
                dialogs.Add(Create_Waterfall3());
                dialogs.Add(Create_Waterfall4());
                dialogs.Add(Create_Waterfall5());

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                await dc.ContinueDialogAsync();

                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test-waterfall-a");
                }
            })
            .Send("hello")
            .AssertReply("step1")
            .AssertReply("step1.1")
            .Send("hello")
            .AssertReply("step1.2")
            .Send("hello")
            .AssertReply("step2")
            .AssertReply("step2.1")
            .Send("hello")
            .AssertReply("step2.2")
            .StartTestAsync();
        }

        [TestMethod]
        public async Task WaterfallDateTimePromptFirstInvalidThenValidInput()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new DateTimePrompt("dateTimePrompt", defaultLocale: Culture.English));
            dialogs.Add(new WaterfallDialog("test-dateTimePrompt", new WaterfallStep[]
            {
                async (stepContext, cancellationToken) =>
                {
                    return await stepContext.PromptAsync("dateTimePrompt", new PromptOptions{ Prompt = new Activity { Text = "Provide a date", Type = ActivityTypes.Message }});
                },
                async (stepContext, cancellationToken) =>
                {
                    Assert.IsNotNull(stepContext);
                    return await stepContext.EndDialogAsync();
                }
            }));

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new AutoSaveStateMiddleware(convoState))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                await dc.ContinueDialogAsync(cancellationToken);

                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test-dateTimePrompt", null, cancellationToken);
                }
            })
            .Send("hello")
            .AssertReply("Provide a date")
            .Send("hello again")
            .AssertReply("Provide a date")
            .Send("Wednesday 4 oclock")
            .StartTestAsync();
        }

        public static WaterfallDialog Create_Waterfall3()
        {
            return new WaterfallDialog("test-waterfall-a", new WaterfallStep[] {
                Waterfall3_Step1,
                Waterfall3_Step2
            });
        }
        public static WaterfallDialog Create_Waterfall4()
        {
            return new WaterfallDialog("test-waterfall-b", new WaterfallStep[] {
                Waterfall4_Step1,
                Waterfall4_Step2
            });
        }

        public static WaterfallDialog Create_Waterfall5()
        {
            return new WaterfallDialog("test-waterfall-c", new WaterfallStep[] {
                Waterfall5_Step1,
                Waterfall5_Step2
            });
        }

        private static async Task<DialogTurnResult> Waterfall3_Step1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("step1"), cancellationToken);
            return await stepContext.BeginDialogAsync("test-waterfall-b", null, cancellationToken);
        }
        private static async Task<DialogTurnResult> Waterfall3_Step2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("step2"), cancellationToken);
            return await stepContext.BeginDialogAsync("test-waterfall-c", null, cancellationToken);
        }

        private static async Task<DialogTurnResult> Waterfall4_Step1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("step1.1"), cancellationToken);
            return Dialog.EndOfTurn;
        }
        private static async Task<DialogTurnResult> Waterfall4_Step2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("step1.2"), cancellationToken);
            return Dialog.EndOfTurn;
        }

        private static async Task<DialogTurnResult> Waterfall5_Step1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("step2.1"), cancellationToken);
            return Dialog.EndOfTurn;
        }
        private static async Task<DialogTurnResult> Waterfall5_Step2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("step2.2"), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }

    public class MyWaterfallDialog : WaterfallDialog
    {
        public MyWaterfallDialog(string id)
            : base(id)
        {
            AddStep(Waterfall2_Step1);
            AddStep(Waterfall2_Step2);
            AddStep(Waterfall2_Step3);
        }

        private static async Task<DialogTurnResult> Waterfall2_Step1(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("step1");
            return Dialog.EndOfTurn;
        }
        private static async Task<DialogTurnResult> Waterfall2_Step2(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("step2");
            return Dialog.EndOfTurn;
        }
        private static async Task<DialogTurnResult> Waterfall2_Step3(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("step3");
            return Dialog.EndOfTurn;
        }
    }

}
