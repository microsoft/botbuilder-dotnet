//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Bot.Builder.Adapters;
//using Microsoft.Bot.Builder.AI.LanguageGeneration;
//using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
//using Microsoft.Bot.Builder.Dialogs.Composition.Resources;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Newtonsoft.Json;

//namespace Microsoft.Bot.Builder.Dialogs.Flow.Tests
//{
//    /// <summary>
//    /// Send dialog id as the reply text when called
//    /// </summary>
//    public class SendIdDialog : Dialog
//    {
//        public SendIdDialog(string id, string altText = null) : base(id)
//        {
//            AltText = altText;
//        }

//        private string AltText { get; set; }

//        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(this.AltText ?? this.Id));
//            return await dc.EndDialogAsync(this.Id);
//        }
//    }

//    /// <summary>
//    /// return the activity text as the result of the dialog
//    /// </summary>
//    public class ReturnTextDialog : Dialog
//    {

//        public ReturnTextDialog(string id) : base(id)
//        {
//        }


//        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            return await dc.EndDialogAsync(dc.Context.Activity.Text);
//        }
//    }

//    /// <summary>
//    /// Echo the activity text back and end
//    /// </summary>
//    public class EchoDialog : Dialog
//    {
//        public EchoDialog(string id) : base(id)
//        {
//        }

//        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(dc.Context.Activity.Text));
//            return await dc.EndDialogAsync(dc.Context.Activity.Text);
//        }
//    }

//    /// <summary>
//    /// Echo the activity text back and end
//    /// </summary>
//    public class SendIdUntilStop : Dialog
//    {
//        public SendIdUntilStop(string id, string altText = null) : base(id)
//        {
//            this.AltText = altText;
//        }

//        public string AltText { get; set; }

//        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            if (dc.Context.Activity.Text == "stop")
//            {
//                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply("stop"));
//                return await dc.EndDialogAsync(dc.Context.Activity.Text);
//            }
//            else
//            {
//                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(this.AltText ?? this.Id));
//                return new DialogTurnResult(DialogTurnStatus.Waiting);
//            }
//        }

//        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            if (dc.Context.Activity.Text == "stop")
//            {
//                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply("stop"));
//                return await dc.EndDialogAsync(dc.Context.Activity.Text);
//            }
//            else
//            {
//                await dc.Context.SendActivityAsync(dc.Context.Activity.CreateReply(this.AltText ?? this.Id));
//                return new DialogTurnResult(DialogTurnStatus.Waiting);
//            }
//        }
//    }


//    [TestClass]
//    public class DialogStepTests
//    {
//        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

//        /// <summary>
//        /// Create test flow
//        /// </summary>
//        private TestAdapter CreateTestAdapter(string initialDialog, IDialog[] dialogs, out BotCallbackHandler botHandler)
//        {
//            var convoState = new ConversationState(new MemoryStorage());
//            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
//            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
//                .Use(new AutoSaveStateMiddleware(convoState))
//                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

//            var dialogSet = new DialogSet(dialogState);
//            foreach (var dialog in dialogs)
//            {
//                dialogSet.Add(dialog);
//            }

//            botHandler = async (turnContext, cancellationToken) =>
//           {
//               var state = await dialogState.GetAsync(turnContext, () => new DialogState());

//               var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

//               var results = await dialogContext.ContinueDialogAsync(cancellationToken);
//               if (results.Status == DialogTurnStatus.Empty)
//                   results = await dialogContext.BeginDialogAsync(initialDialog, null, cancellationToken);
//           };

//            return adapter;
//        }

//        public TestContext TestContext { get; set; }


//        [TestMethod]
//        public async Task CallDialog_Test()
//        {
//            var testDialog = new SequenceDialog()
//            {
//                Id = "TestDialog",
//                Sequence = new Sequence()
//                {
//                    new CallDialog() { Dialog = new SendIdDialog("OneDialog")},
//                    new CallDialog() { Dialog = new SendIdUntilStop("TwoDialog")},
//                },
//            };

//            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("OneDialog")
//                .AssertReply("TwoDialog")
//                .Send("hello")
//                .AssertReply("TwoDialog")
//                .Send("stop")
//                .AssertReply("stop")
//                .Send("hello")
//                .AssertReply("OneDialog")
//                .AssertReply("TwoDialog")
//                .StartTestAsync();
//        }

//        [TestMethod]
//        public async Task CallDialog_NoIdTest()
//        {
//            var testDialog = new SequenceDialog()
//            {
//                Id = "TestDialog",
//                Sequence = new Sequence()
//                {
//                    new CallDialog() { Dialog = new SendIdDialog(null, "OneDialog")},
//                    new CallDialog() { Dialog = new SendIdUntilStop(null, "TwoDialog")},
//                },
//            };

//            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("OneDialog")
//                .AssertReply("TwoDialog")
//                .Send("hello")
//                .AssertReply("TwoDialog")
//                .Send("stop")
//                .AssertReply("stop")
//                .Send("hello")
//                .AssertReply("OneDialog")
//                .AssertReply("TwoDialog")
//                .StartTestAsync();
//        }
//        [TestMethod]
//        public async Task GotoDialog_Test()
//        {
//            var oneDialog = new SendIdDialog("OneDialog");
//            var twoDialog = new SendIdUntilStop("TwoDialog");
//            var threeDialog = new SendIdDialog("ThreeDialog");

//            // when oneDialog finishes, call TwoDialog
//            var testDialog = new SequenceDialog()
//            {
//                Id = "TestDialog",
//                Sequence = new Sequence()
//                    {
//                        new CallDialog() { Id="Start", Dialog = oneDialog },
//                        new GotoDialog() { Dialog = twoDialog },
//                    }
//            };
//            testDialog.AddDialog(oneDialog);
//            testDialog.AddDialog(twoDialog);
//            testDialog.AddDialog(threeDialog);

//            var testAdapter = CreateTestAdapter("TestDialog", new IDialog[] { testDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("OneDialog")
//                .AssertReply("TwoDialog")
//                .Send("hello")
//                .AssertReply("TwoDialog")
//                .Send("stop")
//                .AssertReply("stop")
//                .StartTestAsync();
//        }

//        [TestMethod]
//        public async Task SetClearVal_Test()
//        {
//            var testDialog = new SequenceDialog()
//            {
//                Id = "TestDialog",
//                Sequence = new Sequence()
//                {
//                    // set the test=123
//                    new SetPropertyStep() { Name = "test", Value = new CommonExpression("123") },
//                    // send the value of test
//                    new SendActivityStep("{test}"),
//                    // set test=
//                    new ClearPropertyStep() { Name = "test" },
//                    // send the value of test
//                    //new SendActivityStep("{test}"),
//                }
//            };

//            var botResourceManager = new BotResourceManager();
//            var lg = new LGLanguageGenerator(botResourceManager);
//            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler)
//                .Use(new RegisterClassMiddleware<ILanguageGenerator>(lg));

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("123")
//                .StartTestAsync();
//        }

//        [TestMethod]
//        public async Task ContinueAndEnd_Test()
//        {

//            var returnTextDlg = new ReturnTextDialog($"ReturnText");
//            var testDialog = new SequenceDialog()
//            {
//                Id = $"TestDialog",
//                Sequence = new Sequence() {
//                    new CallDialog() { Dialog = returnTextDlg },
//                    new SwitchStep()
//                    {
//                        Condition = new CommonExpression("DialogTurnResult"),
//                        Cases = new Dictionary<string, IStep>
//                        {
//                            // case "end" 
//                            { "end", new SendActivityStep("Done") },
//                        },
//                        // keep running the dialog
//                        DefaultAction = new EndOfTurnStep()
//                    }
//                }
//            };
//            testDialog.AddDialog(returnTextDlg);
//            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello") // ContinueDialog()
//                .Send("three") // ContinueDialog()
//                .Send("end")   // trigger EndDialog()
//                .AssertReply("Done")
//                .StartTestAsync();
//        }

//        [TestMethod]
//        public async Task NoCommand_Test()
//        {

//            var oneDialog = new SendIdDialog("OneDialog");
//            var testDialog = new SequenceDialog()
//            {
//                Id = "TestDialog",
//                Sequence = new Sequence()
//                {
//                    { new CallDialog() { Dialog = oneDialog } },
//                }
//            };
//            testDialog.AddDialog(oneDialog);

//            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("OneDialog")
//                .StartTestAsync();
//        }

//        [TestMethod]
//        public async Task NoDialog_Test()
//        {
//            var testDialog = new SequenceDialog()
//            {
//                Id = "TestDialog",
//                // no dialog is same as dialog completing
//                // CallDialogId = null
//                Sequence = new Sequence()
//                {
//                    new SendActivityStep("done")
//                }
//            };
//            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("done")
//                .StartTestAsync();
//        }

//        [TestMethod]
//        public async Task SendActivity_Test()
//        {

//            var testDialog = new SequenceDialog()
//            {
//                Id = "TestDialog",
//                // CallDialogId = null
//                Sequence = new Sequence()
//                {
//                    new SendActivityStep("done")
//                }
//            };
//            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("done")
//                .StartTestAsync();
//        }

//        [TestMethod]
//        public async Task Switch_Test()
//        {

//            var echoDialog = new EchoDialog($"EchoDialog");
//            var testDialog = new SequenceDialog()
//            {
//                Id = $"TestDialog",
//                Sequence = new Sequence()
//                {
//                    new CallDialog() { Dialog = echoDialog},
//                    new SwitchStep()
//                    {
//                        Condition = new CommonExpression("DialogTurnResult"),
//                        Cases = new Dictionary<string, IStep>
//                                {
//                                    { $"one", new SendActivityStep("response:1") },
//                                    { $"two", new SendActivityStep("response:2") },
//                                    { $"three", new SendActivityStep("response:3") },
//                                    { $"four", new SendActivityStep("response:4") },
//                                    { $"five", new SendActivityStep("response:5") }
//                                },
//                        DefaultAction = new SendActivityStep("response:default")
//                    }
//                }
//            };
//            testDialog.AddDialog(echoDialog);

//            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("hello")
//                .AssertReply("response:default")
//                .Send("three")
//                .AssertReply("three")
//                .AssertReply("response:3")
//                .Send("five")
//                .AssertReply("five")
//                .AssertReply("response:5")
//                .StartTestAsync();
//        }

//        [TestMethod]
//        public async Task IfElse_Test()
//        {

//            var echoDialog = new EchoDialog($"EchoDialog");
//            var testDialog = new SequenceDialog()
//            {
//                Id = $"TestDialog",
//                Sequence = new Sequence()
//                {
//                    new CallDialog() { Dialog = echoDialog},
//                    new IfElseStep()
//                    {
//                        Condition = new CommonExpression("DialogTurnResult == 'hello'"),
//                        IfTrue = new SendActivityStep("trueResult"),
//                        IfFalse = new SendActivityStep("falseResult"),
//                    }
//                }
//            };
//            testDialog.AddDialog(echoDialog);

//            var testAdapter = CreateTestAdapter("TestDialog", new[] { testDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("hello")
//                .AssertReply("trueResult")
//                .Send("goodbye")
//                .AssertReply("goodbye")
//                .AssertReply("falseResult")
//                .StartTestAsync();
//        }

//        [TestMethod]
//        public async Task FindDialog_Test()
//        {
//            var oneDialog = new SendIdDialog("OneDialog");
//            var twoDialog = new SendIdUntilStop("TwoDialog");
//            var threeDialog = new SendIdDialog("ThreeDialog");
//            var testDialog = new SequenceDialog()
//            {
//                Id = "TestDialog",
//                Sequence = new Sequence()
//                {
//                    new CallDialog() { Id = "Start", Dialog = oneDialog},
//                    new CallDialog() { Dialog = twoDialog },
//                    new CallDialog() { Dialog = threeDialog },
//                },
//            };
//            testDialog.AddDialog(threeDialog);

//            var testAdapter = CreateTestAdapter("TestDialog", new IDialog[] { testDialog, oneDialog, twoDialog }, out var botHandler);

//            await new TestFlow(testAdapter, botHandler)
//                .Send("hello")
//                .AssertReply("OneDialog")
//                .AssertReply("TwoDialog")
//                .Send("hello")
//                .AssertReply("TwoDialog")
//                .Send("stop")
//                .AssertReply("stop")
//                .AssertReply("ThreeDialog")
//                .Send("hello")
//                .AssertReply("OneDialog")
//                .AssertReply("TwoDialog")
//                .StartTestAsync();
//        }
//    }

//}
