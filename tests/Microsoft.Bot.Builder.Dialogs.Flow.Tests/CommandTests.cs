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
    public class CommandTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Create test flow
        /// </summary>
        private static TestAdapter CreateTestAdapter(string initialDialog, out DialogSet dialogs, out BotCallbackHandler botHandler)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger()))
                .Use(new AutoSaveStateMiddleware(convoState));
            var dlgs = new DialogSet(dialogState);
            dialogs = dlgs;
            botHandler = async (turnContext, cancellationToken) =>
           {
               var state = await dialogState.GetAsync(turnContext, () => new DialogState());

               var dialogContext = await dlgs.CreateContextAsync(turnContext, cancellationToken);

               var results = await dialogContext.ContinueDialogAsync(cancellationToken);
               if (results.Status == DialogTurnStatus.Empty)
                   results = await dialogContext.BeginDialogAsync(initialDialog, null, cancellationToken);
           };

            return adapter;
        }



        [TestMethod]
        public async Task CallDialog_Test()
        {
            var testAdapter = CreateTestAdapter("TestDialog", out var dialogs, out var botHandler);

            dialogs.Add(new SendIdDialog("OneDialog"));
            dialogs.Add(new SendIdUntilStop("TwoDialog"));
            dialogs.Add(new SendIdDialog("ThreeDialog"));

            // when oneDialog finishes, call TwoDialog
            var flowDialog = new CommandDialog()
            {
                Id = "TestDialog",
                DialogId = "OneDialog",
                OnCompleted = new CallDialog("TwoDialog")
            };
            dialogs.Add(flowDialog);

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
            var testAdapter = CreateTestAdapter("TestDialog", out var dialogs, out var botHandler);

            dialogs.Add(new SendIdDialog("OneDialog"));
            dialogs.Add(new SendIdUntilStop("TwoDialog"));
            dialogs.Add(new SendIdDialog("ThreeDialog"));


            // when oneDialog finishes, call TwoDialog
            var flowDialog = new CommandDialog()
            {
                Id = "TestDialog",
                DialogId = "OneDialog",
                OnCompleted = new GotoDialog("TwoDialog")
            };
            dialogs.Add(flowDialog);

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
            var testAdapter = CreateTestAdapter("TestDialog", out var dialogs, out var botHandler);

            dialogs.Add(new SendIdDialog("OneDialog"));
            var flowDialog = new CommandDialog()
            {
                Id = "TestDialog",
                DialogId = "OneDialog",
                OnCompleted = new CommandSet()
                {
                    Commands = {
                        // set the test=123
                        new SetVariable() { Name = "test", Value=new CommonExpression("123") },
                        // send the value of test
                        new SendActivity("{test}"),
                        // set test=
                        new ClearVar() { Name = "test" },
                        // send the value of test
                        new SendActivity("{test}"),
                    }
                }
            };
            dialogs.Add(flowDialog);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("OneDialog")
                .AssertReply("123")
                .AssertReply("null")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task ContinueAndEnd_Test()
        {
            var testAdapter = CreateTestAdapter("TestDialog", out var dialogs, out var botHandler);

            dialogs.Add(new ReturnTextDialog($"ReturnText"));
            var flowDialog = new CommandDialog()
            {
                Id = $"TestDialog",
                DialogId = "ReturnText",
                OnCompleted = new Switch()
                {
                    Condition = new CommonExpression("DialogTurnResult.Result"),
                    Cases = new Dictionary<string, IDialogCommand>
                    {
                        // case "end" 
                        {  $"end", new CommandSet() {
                            Commands = new List<IDialogCommand>
                            {
                                // send done
                                new SendActivity("Done"),
                                // end the dialog
                                new EndDialog()
                            }
                        } }
                    },
                    // keep running the dialog
                    DefaultAction = new ContinueDialog()
                }
            };
            dialogs.Add(flowDialog);

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
            var testAdapter = CreateTestAdapter("TestDialog", out var dialogs, out var botHandler);

            dialogs.Add(new SendIdDialog("OneDialog"));
            var flowDialog = new CommandDialog()
            {
                Id = "TestDialog",
                DialogId = "OneDialog"
                // OnCommand = null
            };
            dialogs.Add(flowDialog);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("OneDialog")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task NoDialog_Test()
        {
            var testAdapter = CreateTestAdapter("TestDialog", out var dialogs, out var botHandler);

            var flowDialog = new CommandDialog()
            {
                Id = "TestDialog",
                // no dialog is same as dialog completing
                // CallDialogId = null
                OnCompleted = new SendActivity("done")
            };
            dialogs.Add(flowDialog);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("done")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task SendActivity_Test()
        {
            var testAdapter = CreateTestAdapter("TestDialog", out var dialogs, out var botHandler);

            var flowDialog = new CommandDialog()
            {
                Id = "TestDialog",
                // CallDialogId = null
                OnCompleted = new SendActivity("done")
            };
            dialogs.Add(flowDialog);

            await new TestFlow(testAdapter, botHandler)
                .Send("hello")
                .AssertReply("done")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Switch_Test()
        {
            var testAdapter = CreateTestAdapter("TestDialog", out var dialogs, out var botHandler);

            dialogs.Add(new EchoDialog($"EchoDialog"));
            var flowDialog = new CommandDialog()
            {
                Id = $"TestDialog",
                DialogId = "EchoDialog",
                OnCompleted = new Switch()
                {
                    Condition = new CommonExpression("DialogTurnResult.Result"),
                    Cases = new Dictionary<string, IDialogCommand>
                            {
                                { $"one", new SendActivity("response:1") },
                                { $"two", new SendActivity("response:2") },
                                { $"three", new SendActivity("response:3") },
                                { $"four", new SendActivity("response:4") },
                                { $"five", new SendActivity("response:5") }
                            },
                    DefaultAction = new SendActivity("response:default")
                }
            };
            dialogs.Add(flowDialog);

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
