using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Tests
{
    /// <summary>
    /// Send dialog id as the reply text when called
    /// </summary>
    public class SendIdDialog : Dialog
    {
        public SendIdDialog(string id) : base(id)
        {
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(this.Id));
            return await dc.EndDialogAsync(this.Id);
        }
    }

    /// <summary>
    /// return the activity text as the result of the dialog
    /// </summary>
    public class ReturnTextDialog : Dialog
    {

        public ReturnTextDialog(string id) : base(id)
        {
        }


        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await dc.EndDialogAsync(dc.Context.Activity.Text);
        }
    }

    /// <summary>
    /// Echo the activity text back and end
    /// </summary>
    public class EchoDialog : Dialog
    {
        public EchoDialog(string id) : base(id)
        {
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(dc.Context.Activity.Text));
            return await dc.EndDialogAsync(dc.Context.Activity.Text);
        }
    }

    /// <summary>
    /// Echo the activity text back and end
    /// </summary>
    public class SendIdUntilStop : Dialog
    {
        public SendIdUntilStop(string id) : base(id)
        {
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc.Context.Activity.Text == "stop")
            {
                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply("stop"));
                return await dc.EndDialogAsync(dc.Context.Activity.Text);
            }
            else
            {
                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(this.Id));
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc.Context.Activity.Text == "stop")
            {
                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply("stop"));
                return await dc.EndDialogAsync(dc.Context.Activity.Text);
            }
            else
            {
                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(this.Id));
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }
        }
    }


    [TestClass]
    public class ActionTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Create test flow
        /// </summary>
        private static TestAdapter CreateTestAdapter(string initialDialog, IDialog[] dialogs, out BotCallbackHandler botHandler)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger()))
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogSet = new DialogSet(dialogState);
            foreach (var dialog in dialogs)
            {
                dialogSet.Add(dialog);
            }

            botHandler = async (turnContext, cancellationToken) =>
           {
               var state = await dialogState.GetAsync(turnContext, () => new DialogState());

               var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

               var results = await dialogContext.ContinueDialogAsync(cancellationToken);
               if (results.Status == DialogTurnStatus.Empty)
                   results = await dialogContext.BeginDialogAsync(initialDialog, null, cancellationToken);
           };

            return adapter;
        }



        [TestMethod]
        public async Task CallDialog_Test()
        {
            var oneDialog = new SendIdDialog("OneDialog");
            var twoDialog = new SendIdUntilStop("TwoDialog");
            var threeDialog = new SendIdDialog("ThreeDialog");

            var testDialog = new SequenceDialog()
            {
                Id = "TestDialog",
                Command = new CommandSet()
                {
                    new CallDialog() { Id="Start", Dialog = oneDialog },
                    new CallDialog() { Dialog = twoDialog },
                    new Waiting(),
                    new GotoCommand() { CommandId = "Start" }
                },
            };
            testDialog.AddDialog(oneDialog);
            testDialog.AddDialog(twoDialog);
            testDialog.AddDialog(threeDialog);

            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("OneDialog")
                .AssertReply("TwoDialog")
                .Send("hello")
                .AssertReply("TwoDialog")
                .Send("stop")
                .AssertReply("stop")
                .Send("hello")
                .AssertReply("OneDialog")
                .AssertReply("TwoDialog")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task GotoDialog_Test()
        {
            var oneDialog = new SendIdDialog("OneDialog");
            var twoDialog = new SendIdUntilStop("TwoDialog");
            var threeDialog = new SendIdDialog("ThreeDialog");

            // when oneDialog finishes, call TwoDialog
            var testDialog = new SequenceDialog()
            {
                Id = "TestDialog",
                Command = new CommandSet()
                    {
                        new CallDialog() { Id="Start", Dialog = oneDialog },
                        new GotoDialog() { Dialog = twoDialog },
                    }
            };
            testDialog.AddDialog(oneDialog);
            testDialog.AddDialog(twoDialog);
            testDialog.AddDialog(threeDialog);

            var testAdapter = CreateTestAdapter("TestDialog", new IDialog[] { testDialog }, out var botHandler);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("OneDialog")
                .AssertReply("TwoDialog")
                .Send("hello")
                .AssertReply("TwoDialog")
                .Send("stop")
                .AssertReply("stop")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task SetClearVal_Test()
        {
            var testDialog = new SequenceDialog()
            {
                Id = "TestDialog",
                Command = new CommandSet()
                {
                    // set the test=123
                    new SetVar() { Name = "test", Value = new CommonExpression("123") },
                    // send the value of test
                    new SendActivity("{test}"),
                    // set test=
                    new ClearVar() { Name = "test" },
                    // send the value of test
                    new SendActivity("{test}"),
                }
            };
            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("123")
                .AssertReply("null")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task ContinueAndEnd_Test()
        {

            var returnTextDlg = new ReturnTextDialog($"ReturnText");
            var testDialog = new SequenceDialog()
            {
                Id = $"TestDialog",
                Command = new CommandSet() {
                    new CallDialog() { Dialog = returnTextDlg },
                    new Switch()
                    {
                        Condition = new CommonExpression("DialogTurnResult"),
                        Cases = new Dictionary<string, IDialogAction>
                        {
                            // case "end" 
                            { "end", new SendActivity("Done") },
                        },
                        // keep running the dialog
                        DefaultAction = new Waiting()
                    }
                }
            };
            testDialog.AddDialog(returnTextDlg);
            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello") // ContinueDialog()
                .Send("three") // ContinueDialog()
                .Send("end")   // trigger EndDialog()
                .AssertReply("Done")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task NoCommand_Test()
        {

            var oneDialog = new SendIdDialog("OneDialog");
            var testDialog = new SequenceDialog()
            {
                Id = "TestDialog",
                Command = new CommandSet()
                {
                    { new CallDialog() { Dialog = oneDialog } },
                }
            };
            testDialog.AddDialog(oneDialog);

            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("OneDialog")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task NoDialog_Test()
        {
            var testDialog = new SequenceDialog()
            {
                Id = "TestDialog",
                // no dialog is same as dialog completing
                // CallDialogId = null
                Command = new CommandSet()
                {
                    new SendActivity("done")
                }
            };
            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("done")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task SendActivity_Test()
        {

            var testDialog = new SequenceDialog()
            {
                Id = "TestDialog",
                // CallDialogId = null
                Command = new CommandSet()
                {
                    new SendActivity("done")
                }
            };
            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("done")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Switch_Test()
        {

            var echoDialog = new EchoDialog($"EchoDialog");
            var testDialog = new SequenceDialog()
            {
                Id = $"TestDialog",
                Command = new CommandSet()
                {
                    new CallDialog() { Dialog = echoDialog},
                    new Switch()
                    {
                        Condition = new CommonExpression("DialogTurnResult"),
                        Cases = new Dictionary<string, IDialogAction>
                                {
                                    { $"one", new SendActivity("response:1") },
                                    { $"two", new SendActivity("response:2") },
                                    { $"three", new SendActivity("response:3") },
                                    { $"four", new SendActivity("response:4") },
                                    { $"five", new SendActivity("response:5") }
                                },
                        DefaultAction = new SendActivity("response:default")
                    }
                }
            };
            testDialog.AddDialog(echoDialog);

            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("hello")
                .AssertReply("response:default")
                .Send("three")
                .AssertReply("three")
                .AssertReply("response:3")
                .Send("five")
                .AssertReply("five")
                .AssertReply("response:5")
                .StartTestAsync();
        }

    }
}
