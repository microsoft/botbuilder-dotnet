// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestBot : IBot
    {
        private DialogSet _dialogs;
        private SemaphoreSlim _semaphore;

        private readonly IDialog rootDialog;
        public TestBot(TestBotAccessors accessors)
        {
            // create the DialogSet from accessor
            rootDialog = DialogLoader.Load(File.ReadAllText("bot.json"));

            _dialogs = new DialogSet(accessors.ConversationDialogState);
            _dialogs.Add(rootDialog);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {


            if (turnContext.Activity.Type == ActivityTypes.Message && turnContext.Activity.Text == "throw")
            {
                throw new Exception("oh dear");
            }

            // run the DialogSet - let the framework identify the current state of the dialog from 
            // the dialog stack and figure out what (if any) is the active dialog
            var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
            var results = await dialogContext.ContinueDialogAsync(cancellationToken);

            // HasActive = true if there is an active dialog on the dialogstack
            // HasResults = true if the dialog just completed and the final  result can be retrived
            // if both are false this indicates a new dialog needs to start
            // an additional check for Responded stops a new waterfall from being automatically started over
            if (results.Status == DialogTurnStatus.Empty || results.Status == DialogTurnStatus.Complete)
            {
                await dialogContext.BeginDialogAsync(rootDialog.Id, null, cancellationToken);
            }

        }
    }
}
