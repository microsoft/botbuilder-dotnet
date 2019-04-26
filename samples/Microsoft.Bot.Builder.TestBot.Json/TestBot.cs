// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Dialogs.Debugging.Source;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestBot : IBot
    {
        private DialogSet _dialogs;
        private IDialog rootDialog;
        private readonly ResourceExplorer resourceExplorer;

        public TestBot(ConversationState conversationState, ResourceExplorer resourceExplorer, Source.IRegistry registry = null)
        {
            _dialogs = new DialogSet(conversationState.CreateProperty<DialogState>("DialogState"));

            this.resourceExplorer = resourceExplorer;
            registry = registry ?? NullRegistry.Instance;

            // auto reload dialogs when file changes
            this.resourceExplorer.Changed += (paths) =>
            {
                if (paths.Any(p => Path.GetExtension(p) == ".dialog"))
                {
                    Task.Run(() => this.LoadRootDialogAsync(registry));
                }
            };

            LoadRootDialogAsync(registry);
        }

        
        private void LoadRootDialogAsync(IRegistry registry)
        {
            System.Diagnostics.Trace.TraceInformation("Loading resources...");
            //var rootFile = resourceExplorer.GetResource(@"VARootDialog.main.dialog");
            // var rootFile = resourceExplorer.GetResource("ToDoLuisBot.main.dialog");
            // var rootFile = resourceExplorer.GetResource(@"ToDoBot.main.dialog");
            //var rootFile = resourceExplorer.GetResource("NoMatchRule.main.dialog");
            //var rootFile = resourceExplorer.GetResource("EndTurn.main.dialog");
            //var rootFile = resourceExplorer.GetResource("IfCondition.main.dialog");
            //var rootFile = resourceExplorer.GetResource("TextInput.main.dialog");
            //var rootFile = resourceExplorer.GetResource("WelcomeRule.main.dialog");
            //var rootFile = resourceExplorer.GetResource("DoSteps.main.dialog");
            //var rootFile = resourceExplorer.GetResource("BeginDialog.main.dialog");
            //var rootFile = resourceExplorer.GetResource("ExternalLanguage.main.dialog");
            //var rootFile = resourceExplorer.GetResource("CustomStep.dialog");
            var rootFile = resourceExplorer.GetResource("CustomStep.dialog");

            rootDialog = DeclarativeTypeLoader.Load<IDialog>(rootFile, resourceExplorer, registry);
            //rootDialog = LoadCodeDialog();

            _dialogs.Add(rootDialog);

            System.Diagnostics.Trace.TraceInformation("Done loading resources.");
        }

        private AdaptiveDialog LoadCodeDialog()
        {
            var expressionParser = new ExpressionEngine();
            var dialog = new AdaptiveDialog()
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new Dictionary<string, string>()
                    {
                        { "Intent1", "intent1" },
                        { "Intent2", "intent2" },
                        { "Intent3", "intent3" },
                        { "Intent4", "intent4" },
                    }
                },
                Steps = new List<IDialog>()
                {
                    new SendActivity("hello1"),
                    new SendActivity("hello2"),
                    new IfCondition()
                    {
                        Condition = expressionParser.Parse("user.name == null"),
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("name is null"),
                        },
                        ElseSteps = new List<IDialog>()
                        {
                            new SendActivity("name is not null"),
                        }
                    },
                    new SendActivity("hello4")
                },
                Rules = new List<IRule>()
                {
                    new IntentRule("Intent1")
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Intent 1 triggered")
                        }
                    },
                    new IntentRule("Intent2")
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Intent 2 triggered")
                        }
                    },
                    new IntentRule("Intent3")
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Intent 3 triggered")
                        }
                    },
                    new UnknownIntentRule()
                    {
                        Steps = new List<IDialog>()
                        {
                            new SendActivity("Wha?")
                        }
                    },
                }
            };

            return dialog;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (rootDialog is AdaptiveDialog planningDialog)
            {
                await planningDialog.OnTurnAsync(turnContext, null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (turnContext.Activity.Type == ActivityTypes.Message && turnContext.Activity.Text == "throw")
                {
                    throw new Exception("oh dear");
                }

                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    // run the DialogSet - let the framework identify the current state of the dialog from 
                    // the dialog stack and figure out what (if any) is the active dialog
                    var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                    var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                    if (results.Status == DialogTurnStatus.Empty || results.Status == DialogTurnStatus.Complete)
                    {
                        await dialogContext.BeginDialogAsync(rootDialog.Id, null, cancellationToken);
                    }
                }
            }
        }
    }
}
