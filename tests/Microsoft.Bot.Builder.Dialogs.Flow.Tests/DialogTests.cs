using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Tests
{
    [TestClass]
    public class DialogTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        public IDialog CreateTestDialog()
        {
            var dialogs = new ComponentDialog() { Id = "TestDialog" };

            // add prompts
            dialogs.AddDialog(new NumberPrompt<Int32>()
            {
                Id = "AgePrompt",
                DefaultOptions = new PromptOptions()
                {
                    Prompt = new Activity(type: ActivityTypes.Message, text: "What is your age?"),
                    RetryPrompt = new Activity(type: ActivityTypes.Message, text: "Reprompt: What is your age?")
                }
            });

            dialogs.AddDialog(new TextPrompt()
            {
                Id = "NamePrompt",
                DefaultOptions = new PromptOptions()
                {
                    Prompt = new Activity(type: ActivityTypes.Message, text: "What is your name?"),
                    RetryPrompt = new Activity(type: ActivityTypes.Message, text: "Reprompt: What is your name?")
                }
            });

            var flowDialog2 = new CommandDialog()
            {
                Id = "FlowDialog2",
                Command = new CommandSet("Dialog2")
                {
                    new CallDialog() { Dialog = dialogs.FindDialog("AgePrompt") },
                    new SetVar() { Name = "Age", Value = new CommonExpression("DialogTurnResult") },
                    new SetVar() { Name = "IsChild", Value = new CommonExpression("Age < 18") },
                    new SendActivity("Done"),
                }
            };
            dialogs.AddDialog(flowDialog2);

            // define GetNameDialog
            var flowDialog = new CommandDialog()
            {
                Id = "FlowDialog",
                Command = new CommandSet("Dialog") {
                    new CallDialog() { Id = "CallNamePrompt", Dialog = dialogs.FindDialog("NamePrompt") },
                    new SetVar() { Name ="Name", Value = new CommonExpression("DialogTurnResult") },
                    new IfElse()
                    {
                        Condition = new CommonExpression() { Expression ="Name.Length > 2" },
                        True = new CallDialog() { Dialog = flowDialog2 },
                        Else = new GotoCommand() { CommandId = "CallNamePrompt" },
                    },
                }
            };
            dialogs.InitialDialogId = flowDialog.Id;
            dialogs.AddDialog(flowDialog);

            return dialogs;
        }

        [TestMethod]
        public async Task TestFlowDialog()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogState = convoState.CreateProperty<DialogState>("dialogState");

            var adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger()))
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogs = new DialogSet(dialogState);

            var testDialog = CreateTestDialog();
            dialogs.Add(testDialog);

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await dialogState.GetAsync(turnContext, () => new DialogState());

                var dialogContext = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                    results = await dialogContext.BeginDialogAsync(testDialog.Id, null, cancellationToken);
            })
            .Send("hello")
            .AssertReply("What is your name?")
            .Send("x")
            .AssertReply("What is your name?")
            .Send("Joe")
            .AssertReply("What is your age?")
            .Send("64")
            .AssertReply("Done")
            .StartTestAsync();
        }

        //{

        //    var sd = new SwitchAction();
        //    sd.Condition = new ReflectionExpression("1 == 1");
        //    sd.Cases.Add("true", new TestAction("true"));
        //    sd.Cases.Add("false", new TestAction("false"));
        //    sd.DefaultAction = new TestAction("default");

        //    Assert.AreEqual(5, results.Count, "Should be 5 entities found");
        //    Assert.AreEqual(1, results.Where(entity => entity.Type == "age").Count(), "Should have 1 age entity");
        //}

    }
}
