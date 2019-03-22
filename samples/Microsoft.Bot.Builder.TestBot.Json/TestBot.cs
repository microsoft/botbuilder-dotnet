// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Rules;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestBot : IBot
    {
        private DialogSet _dialogs;

        private readonly IDialog rootDialog;

        private readonly IBotResourceProvider resourceProvider;
        
        public TestBot(TestBotAccessors accessors, IBotResourceProvider resourceProvider)
        {
            rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Southworks - StandUp\StandUp.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning - ToDoLuisBot\ToDoLuisBot.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\RootDialog\RootDialog.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 1 - DefaultRule\DefaultRule.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 2 - WaitForInput\WaitForInput.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 3 - IfProperty\IfProperty.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 4 - TextPrompt\TextPrompt.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 5 - WelcomeRule\WelcomeRule.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 6 - DoSteps\DoSteps.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 7 - CallDialog\CallDialog.main.dialog"), resourceProvider);
            //rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 8 - ExternalLanguage\ExternalLanguage.main.dialog"), resourceProvider);

            _dialogs = new DialogSet(accessors.ConversationDialogState);
            _dialogs.Add(rootDialog);
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
