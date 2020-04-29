// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Skills;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestBot : ActivityHandler
    {
        private IStatePropertyAccessor<DialogState> dialogStateAccessor;
        private DialogManager dialogManager;
        private readonly ResourceExplorer resourceExplorer;

        public TestBot(ConversationState conversationState, ResourceExplorer resourceExplorer, BotFrameworkClient skillClient, SkillConversationIdFactoryBase conversationIdFactory)
        {
            HostContext.Current.Set(skillClient);
            HostContext.Current.Set(conversationIdFactory);
            this.dialogStateAccessor = conversationState.CreateProperty<DialogState>("RootDialogState");
            this.resourceExplorer = resourceExplorer;

            // auto reload dialogs when file changes
            this.resourceExplorer.Changed += (e, resources) =>
            {
                if (resources.Any(resource => resource.Id.EndsWith(".dialog") || resource.Id.EndsWith(".lg")))
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

            // Create a non-used dialog just to make sure the target assembly is referred so that
            // the target assembly's component registration can be used to deserialize declarative components
            var qnaDialog = new QnAMakerDialog();
            System.Diagnostics.Trace.TraceInformation($"Touch ${qnaDialog.GetType().ToString()} to make sure assembly is referred.");

            var rootDialog = new AdaptiveDialog()
            {
                AutoEndDialog = false,
            };
            var choiceInput = new ChoiceInput()
            {
                Prompt = new ActivityTemplate("What declarative sample do you want to run?"),
                Property = "conversation.dialogChoice",
                AlwaysPrompt = true,
                Choices = new ChoiceSet(new List<Choice>())
            };

            var handleChoice = new SwitchCondition()
            {
                Condition = "conversation.dialogChoice",
                Cases = new List<Case>()
            };

            Dialog lastDialog = null;
            var choices = new ChoiceSet();

            foreach (var resource in this.resourceExplorer.GetResources(".dialog").Where(r => r.Id.EndsWith(".main.dialog")))
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(resource.Id));
                    choices.Add(new Choice(name));
                    var dialog = resourceExplorer.LoadType<Dialog>(resource);
                    lastDialog = dialog;
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

            if (handleChoice.Cases.Count() == 1)
            {
                rootDialog.Triggers.Add(new OnBeginDialog
                {
                    Actions = new List<Dialog>
                    {
                        lastDialog,
                        new RepeatDialog()
                    }
                });
            }
            else
            {
                choiceInput.Choices = choices;
                choiceInput.Style = ListStyle.Auto;
                rootDialog.Triggers.Add(new OnBeginDialog()
                {
                    Actions = new List<Dialog>()
                {
                    choiceInput,
                    new SendActivity("# Running ${conversation.dialogChoice}.main.dialog"),
                    handleChoice,
                    new RepeatDialog()
                }
                });
            }

            this.dialogManager = new DialogManager(rootDialog)
                .UseResourceExplorer(this.resourceExplorer)
                .UseLanguageGeneration();

            Trace.TraceInformation("Done loading resources.");
        }
    }
}
