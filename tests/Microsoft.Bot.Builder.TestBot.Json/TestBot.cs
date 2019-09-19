// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestBot : ActivityHandler
    {
        private IStatePropertyAccessor<DialogState> dialogStateAccessor;
        private DialogManager dialogManager;
        private readonly ResourceExplorer resourceExplorer;

        public TestBot(ConversationState conversationState, ResourceExplorer resourceExplorer)
        {
            this.dialogStateAccessor = conversationState.CreateProperty<DialogState>("RootDialogState");
            this.resourceExplorer = resourceExplorer;

            // auto reload dialogs when file changes
            this.resourceExplorer.Changed += (resources) =>
            {
                if (resources.Any(resource => resource.Id.EndsWith(".dialog")))
                {
                    Task.Run(() => this.LoadDialogs());
                }
            };
            LoadDialogs();
        }

        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.dialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken);
        }

        private void LoadDialogs()
        {
            System.Diagnostics.Trace.TraceInformation("Loading resources...");

            var rootDialog = new AdaptiveDialog()
            {
                AutoEndDialog = false,
            };
            var choiceInput = new ChoiceInput()
            {
                Prompt = new ActivityTemplate("What declarative sample do you want to run?"),
                Property = "conversation.dialogChoice",
                AlwaysPrompt = true,
                Choices = new List<Choice>()
            };

            var handleChoice = new SwitchCondition()
            {
                Condition = "conversation.dialogChoice",
                Cases = new List<Case>()
            };

            foreach (var resource in this.resourceExplorer.GetResources(".dialog").Where(r => r.Id.EndsWith(".main.dialog")))
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(resource.Id));
                    choiceInput.Choices.Add(new Choice(name));
                    var dialog = DeclarativeTypeLoader.Load<Dialog>(resource, this.resourceExplorer, DebugSupport.SourceRegistry);
                    handleChoice.Cases.Add(new Case($"{name}", new List<Dialog>() { dialog }));
                }
                catch (SyntaxErrorException err)
                {
                    Trace.TraceError($"{err.Source}: Error: {err.Message}");
                }
                catch (Exception err)
                {
                    Trace.TraceError(err.Message);
                }
            }

            choiceInput.Style = ListStyle.Auto;
            rootDialog.Triggers.Add(new OnBeginDialog()
            {
                Actions = new List<Dialog>()
                {
                    choiceInput,
                    new SendActivity("# Running {conversation.dialogChoice}.main.dialog"),
                    handleChoice,
                    new RepeatDialog()
                }
            });

            this.dialogManager = new DialogManager(rootDialog);

            System.Diagnostics.Trace.TraceInformation("Done loading resources.");
        }
    }
}
