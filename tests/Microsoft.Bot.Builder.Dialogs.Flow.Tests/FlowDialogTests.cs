using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Tests
{
    [TestClass]
    public class FlowDialogTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        public IDialog CreateTestDialog()
        {
            var dialog = new ComponentDialog() { Id = "TestDialog" };

            // add prompts
            dialog.AddDialog(new NumberPrompt<Int32>());
            dialog.AddDialog(new TextPrompt()
            {
                Id = "NamePrompt",
                Options = new PromptOptions()
                {
                    Prompt = new Activity(type: ActivityTypes.Message, text: "What is your name?"),
                    RetryPrompt = new Activity(type: ActivityTypes.Message, text: "Reprompt: What is your name?")
                }
            });

            // define GetNameDialog
            var flowDialog = new FlowDialog()
            {
                Id = "GetNameDialog",
                CallDialogId = "NamePrompt",
                OnCompleted = new CommandSet()
                {
                    Commands = {
                        new SetVariable() { Name="Name", Value= new CSharpExpression("State.DialogTurnResult.Result")},
                        new Switch()
                        {
                            Condition = new CSharpExpression() { Expression="State.Name.Length > 2" },
                            Cases = new Dictionary<string, IDialogCommand>
                            {
                                { "true", new CallDialog("GetAgeDialog")  },
                                { "false", new CallDialog("GetNameDialog") }
                            },
                            DefaultAction = new SendActivity("default")
                        }
                    }
                }
            };
            dialog.InitialDialogId = flowDialog.Id;
            dialog.AddDialog(flowDialog);

            // define GetAgeDialog
            flowDialog = new FlowDialog()
            {
                Id = "GetAgeDialog",
                CallDialogId = "NumberPrompt",
                CallDialogOptions = new PromptOptions()
                {
                    Prompt = new Activity(type: ActivityTypes.Message, text: "What is your age?"),
                    RetryPrompt = new Activity(type: ActivityTypes.Message, text: "Reprompt: What is your age?")
                },
                OnCompleted = new CommandSet()
                {
                    Commands = {
                        new SetVariable() { Name = "Age", Value = new CSharpExpression("State.DialogTurnResult.Result") },
                        new SetVariable() { Name = "IsChild", Value = new CSharpExpression("State.Age < 18") },
                        new SendActivity() { Text = "Done" }
                    }
                }
            };
            dialog.AddDialog(flowDialog);

            return dialog;
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
