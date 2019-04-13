// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestBot : IBot
    {
        private DialogSet _dialogs;

        private IDialog rootDialog;

        private readonly ResourceExplorer resourceExplorer;

        private UserState userState;
        private ConversationState conversationState;
        private IStatePropertyAccessor<DialogState> dialogState;

        private Source.IRegistry registry;

        public TestBot(UserState userState, ConversationState conversationState, ResourceExplorer resourceExplorer, Source.IRegistry registry)
        {
            dialogState = conversationState.CreateProperty<DialogState>("DialogState");

            this.registry = registry;
            this.resourceExplorer = resourceExplorer;
            
            // auto reload dialogs when file changes
            this.resourceExplorer.Changed += ResourceExplorer_Changed;

            LoadRootDialog();
        }

        private void ResourceExplorer_Changed(string[] paths)
        {
            if (paths.Any(p => Path.GetExtension(p) == ".dialog"))
            {
                this.LoadRootDialog();
            }
        }

        
        private void LoadRootDialog()
        {
            System.Diagnostics.Trace.TraceInformation("Loading resources...");
            //var rootFile = resourceExplorer.GetResource(@"ToDoBot.main.dialog");
            var rootFile = resourceExplorer.GetResource("ToDoLuisBot.main.dialog");
            //var rootFile = resourceExplorer.GetResource("NoMatchRule.main.dialog");
            //var rootFile = resourceExplorer.GetResource("EndTurn.main.dialog");
            //var rootFile = resourceExplorer.GetResource("IfCondition.main.dialog");
            //var rootFile = resourceExplorer.GetResource("TextInput.main.dialog");
            //var rootFile = resourceExplorer.GetResource("WelcomeRule.main.dialog");
            //var rootFile = resourceExplorer.GetResource("DoSteps.main.dialog");
            //var rootFile = resourceExplorer.GetResource("BeginDialog.main.dialog");
            //var rootFile = resourceExplorer.GetResource("ExternalLanguage.main.dialog");
            //var rootFile = resourceExplorer.GetResource("CustomStep.dialog");

            rootDialog = DeclarativeTypeLoader.Load<IDialog>(rootFile.FullName, resourceExplorer, registry);
            _dialogs = new DialogSet(this.dialogState);
            _dialogs.Add(rootDialog);

            System.Diagnostics.Trace.TraceInformation("Done loading resources.");
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
