using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Tests
{
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


    [TestClass]
    public class DialogCommandTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        private static TestFlow CreateTestFlow(string dialogId, out DialogSet dialogs)
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger()))
                .Use(new AutoSaveStateMiddleware(convoState));
            var dlgs = new DialogSet(dialogState);
            dialogs = dlgs;

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());

                var dialogContext = await dlgs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                    results = await dialogContext.BeginDialogAsync(dialogId, null, cancellationToken);
            });
        }


        [TestMethod]
        public async Task CallDialog_Test()
        {
            var testFlow = CreateTestFlow("Step1", out var dialogs);

            var flowDialog = new FlowDialog() { Id = "Step1", CallDialogId = "OneDialog", OnCompleted = new CallDialog("TwoDialog") };
            dialogs.Add(flowDialog);
            dialogs.Add(new SendIdDialog("OneDialog"));
            dialogs.Add(new SendIdDialog("TwoDialog"));

            await testFlow.Send("hello")
                .AssertReply("OneDialog")
                .AssertReply("TwoDialog")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task SetClearVal_Test()
        {
            var testFlow = CreateTestFlow("Step1", out var dialogs);

            var flowDialog = new FlowDialog()
            {
                Id = "Step1",
                CallDialogId = "OneDialog",
                OnCompleted = new CommandSet()
                {
                    Commands = {
                        new SetVariable() { Name = "test", Value=new CSharpExpression("123") },
                        new SendActivity("{test}"),
                        new ClearVar() { Name = "test" },
                        new SendActivity("{test}"),
                    }
                }
            };
            dialogs.Add(new SendIdDialog("OneDialog"));
            dialogs.Add(flowDialog);

            await testFlow.Send("hello")
                .AssertReply("OneDialog")
                .AssertReply("123")
                .AssertReply("null")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task ContinueAndEnd_Test()
        {
            var testFlow = CreateTestFlow("Step1", out var dialogs);

            var flowDialog = new FlowDialog()
            {
                Id = $"Step1",
                CallDialogId = "ReturnText",
                OnCompleted = new Switch()
                {
                    Condition = new CSharpExpression("State.DialogTurnResult.Result"),
                    Cases = new Dictionary<string, IDialogCommand>
                    {
                        {  $"end", new CommandSet() {
                            Commands = new List<IDialogCommand>
                            {
                                new SendActivity("Done"),
                                new EndDialog()
                            }
                        } }
                    },
                    DefaultAction = new ContinueDialog()
                }
            };
            dialogs.Add(new ReturnTextDialog($"ReturnText"));
            dialogs.Add(flowDialog);

            await testFlow
                .Send("hello") // ContinueDialog()
                .Send("three") // ContinueDialog()
                .Send("end") // EndDialog()
                .AssertReply("Done")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task NoCommand_Test()
        {
            var testFlow = CreateTestFlow("Step1", out var dialogs);

            var flowDialog = new FlowDialog()
            {
                Id = "Step1",
                CallDialogId = "OneDialog"
            };
            dialogs.Add(flowDialog);
            dialogs.Add(new SendIdDialog("OneDialog"));

            await testFlow.Send("hello")
                .AssertReply("OneDialog")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task NoDialog_Test()
        {
            var testFlow = CreateTestFlow("Step1", out var dialogs);

            var flowDialog = new FlowDialog()
            {
                Id = "Step1",
                OnCompleted = new SendActivity("done")
            };
            dialogs.Add(flowDialog);
            dialogs.Add(new SendIdDialog("OneDialog"));

            await testFlow.Send("hello")
                .AssertReply("done")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task Switch_Test()
        {
            var testFlow = CreateTestFlow("Step1", out var dialogs);

            var flowDialog = new FlowDialog()
            {
                Id = $"Step1",
                CallDialogId = "EchoDialog",
                OnCompleted = new Switch()
                {
                    Condition = new CSharpExpression("State.DialogTurnResult.Result"),
                    Cases = new Dictionary<string, IDialogCommand>
                            {
                                { $"one", new SendActivity("1") },
                                { $"two", new SendActivity("2") },
                                { $"three", new SendActivity("3") },
                                { $"four", new SendActivity("4") },
                                { $"five", new SendActivity("5") }
                            },
                    DefaultAction = new SendActivity("default")
                }
            };
            dialogs.Add(new EchoDialog($"EchoDialog"));
            dialogs.Add(flowDialog);

            await testFlow
                .Send("hello")
                .AssertReply("hello")
                .AssertReply("default")
                .Send("three")
                .AssertReply("three")
                .AssertReply("3")
                .Send("five")
                .AssertReply("five")
                .AssertReply("5")
                .StartTestAsync();
        }

    }
}
