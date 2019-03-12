//using System;
//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.Bot.Builder.Adapters;
//using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
//using Microsoft.Bot.Schema;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Newtonsoft.Json;

//namespace Microsoft.Bot.Builder.Dialogs.Flow.Tests
//{
//    [TestClass]
//    public class SequenceDialogTests
//    {
//        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

//        public TestContext TestContext { get; set; }

//        public IDialog CreateTestDialog()
//        {
//            // add prompts
//            var agePrompt = new NumberPrompt<Int32>()
//            {
//                Id = "AgePrompt",
//                InitialPrompt = new ActivityTemplate("What is your age?"),
//                RetryPrompt = new ActivityTemplate("Reprompt: What is your age?")
//            };

//            var namePrompt = new TextPrompt()
//            {
//                Id = "NamePrompt",
//                InitialPrompt = new ActivityTemplate("What is your name?"),
//                RetryPrompt = new ActivityTemplate("Reprompt: What is your name?")
//            };

//            var flowDialog2 = new SequenceDialog()
//            {
//                Id = "FlowDialog2",
//                Sequence = new Sequence("Dialog2")
//                {
//                    new CallDialog() { Dialog = agePrompt },
//                    new SetPropertyStep() { Name = "Age", Value = new CommonExpression("DialogTurnResult") },
//                    new SetPropertyStep() { Name = "IsChild", Value = new CommonExpression("Age < 18") },
//                    new SendActivityStep("Done"),
//                }
//            };
//            flowDialog2.AddDialog(agePrompt);
//            // define GetNameDialog
//            var flowDialog = new SequenceDialog()
//            {
//                Id = "FlowDialog",
//                Sequence = new Sequence("Dialog") {
//                    new CallDialog() { Id = "CallNamePrompt", Dialog = namePrompt },
//                    new SetPropertyStep() { Name ="Name", Value = new CommonExpression("DialogTurnResult") },
//                    new IfElseStep()
//                    {
//                        Condition = new CommonExpression() { Expression ="Name.Length > 2" },
//                        IfTrue = new CallDialog() { Dialog = flowDialog2 },
//                        IfFalse = new GotoStep() { CommandId = "CallNamePrompt" },
//                    },
//                }
//            };
//            flowDialog.AddDialog(namePrompt);
//            flowDialog.AddDialog(flowDialog2);

//            return flowDialog;
//        }

//        [TestMethod]
//        public async Task TestFlowDialog()
//        {
//            var convoState = new ConversationState(new MemoryStorage());
//            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

//            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
//                .Use(new AutoSaveStateMiddleware(convoState))
//                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

//            var dialogs = new DialogSet(dialogState);

//            var testDialog = CreateTestDialog();
//            dialogs.Add(testDialog);

//            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
//            {
//                var state = await dialogState.GetAsync(turnContext, () => new DialogState());

//                var dialogContext = await dialogs.CreateContextAsync(turnContext, cancellationToken);

//                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
//                if (results.Status == DialogTurnStatus.Empty)
//                    results = await dialogContext.BeginDialogAsync(testDialog.Id, null, cancellationToken);
//            })
//            .Send("hello")
//            .AssertReply("What is your name?")
//            .Send("x")
//            .AssertReply("What is your name?")
//            .Send("Joe")
//            .AssertReply("What is your age?")
//            .Send("64")
//            .AssertReply("Done")
//            .StartTestAsync();
//        }

//        //{

//        //    var sd = new SwitchAction();
//        //    sd.Condition = new ReflectionExpression("1 == 1");
//        //    sd.Cases.Add("true", new TestAction("true"));
//        //    sd.Cases.Add("false", new TestAction("false"));
//        //    sd.DefaultAction = new TestAction("default");

//        //    Assert.AreEqual(5, results.Count, "Should be 5 entities found");
//        //    Assert.AreEqual(1, results.Where(entity => entity.Type == "age").Count(), "Should have 1 age entity");
//        //}

//    }
//}
