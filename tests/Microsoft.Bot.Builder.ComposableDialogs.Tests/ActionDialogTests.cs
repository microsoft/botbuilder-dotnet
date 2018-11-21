using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.ComposableDialogs.Dialogs;
using Microsoft.Bot.Builder.ComposableDialogs.Expressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.ComposableDialogs.Tests
{
    [TestClass]
    public class ActionDialogTests
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        public IDialog CreateTestDialog()
        {
            var dialog = new ComponentDialog() { Id = "TestDialog" };

            // add prompts
            dialog.AddDialog(new NumberPrompt<Int32>());
            dialog.AddDialog(new TextPrompt());

            // define GetNameDialog
            var actionDialog = new ActionDialog("GetNameDialog")
            {
                DialogId = "TextPrompt",
                DialogOptions = new PromptOptions()
                {
                    Prompt = new Activity(type: ActivityTypes.Message, text: "What is your name?"),
                    RetryPrompt = new Activity(type: ActivityTypes.Message, text: "What is your name?")
                },
                OnCompleted = new ActionSet()
                {
                    Actions =
                    {
                        new SetVarAction() { Name="Name", Value= new CSharpExpression("State.DialogTurnResult.Result")},
                        new SwitchAction()
                        {
                            Condition = new CSharpExpression() { Expression="State.Name.Length > 2" },
                            Cases = new Dictionary<string, IAction>
                            {
                                { "true", new CallDialogAction("GetAgeDialog")  },
                                { "false", new ContinueDialogAction() }
                            },
                            DefaultAction = new SendActivityAction("default")
                        }
                    }
                }
            };
            dialog.InitialDialogId = actionDialog.Id;
            dialog.AddDialog(actionDialog);

            // define GetAgeDialog
            actionDialog = new ActionDialog("GetAgeDialog")
            {
                DialogId = "NumberPrompt",
                DialogOptions = new PromptOptions()
                {
                    Prompt = new Activity(type: ActivityTypes.Message, text: "What is your age?"),
                    RetryPrompt = new Activity(type: ActivityTypes.Message, text: "What is your age?")
                },
                OnCompleted = new ActionSet()
                {
                    Actions = {
                        new SetVarAction() { Name = "Age", Value = new CSharpExpression("State.DialogTurnResult.Result") },
                        new SetVarAction() { Name = "IsChild", Value = new CSharpExpression("State.Age < 18") },
                        new SendActivityAction() { Text = "Done" }
                    }
                }
            };
            dialog.AddDialog(actionDialog);

            return dialog;
        }

        [TestMethod]
        public async Task TestActionDialog()
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
