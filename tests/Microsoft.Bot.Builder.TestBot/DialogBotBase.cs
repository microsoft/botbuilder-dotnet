// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.TestBot
{
    public class DialogBotBase : IBot
    {
        private SemaphoreSlim _semaphore;
        private IBotStore _state;
        private string _rootDialogName;

        public DialogBotBase(TestBotAccessors accessors, string rootDialogName)
        {
            // create the DialogSet from accessor
            Dialogs = new DialogSet(accessors.ConversationDialogState);

            // a semaphore to serialize access to the bot state
            _semaphore = accessors.SemaphoreSlim;

            // the state to attempt to save at the end of every turn
            _state = accessors.State;

            // the name of the root dialog this bot runs
            _rootDialogName = rootDialogName;
        }

        protected DialogSet Dialogs { get; private set; }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // We only want to pump one activity at a time through the state.
                // Note the state is shared across all instances of this IBot class so we
                // create the semaphore globally with the accessors.
                try
                {
                    await _semaphore.WaitAsync();

                    // explicit load of the bot state
                    await _state.LoadAsync(turnContext, cancellationToken);

                    // run the DialogSet - let the framework identify the current state of the dialog from 
                    // the dialog stack and figure out what (if any) is the active dialog
                    var dialogContext = await Dialogs.CreateContextAsync(turnContext, cancellationToken);
                    var results = await dialogContext.ContinueAsync(cancellationToken);

                    // HasActive = true if there is an active dialog on the dialogstack
                    // HasResults = true if the dialog just completed and the final  result can be retrived
                    // if both are false this indicates a new dialog needs to start
                    // an additional check for Responded stops a new waterfall from being automatically started over
                    if (!turnContext.Responded && !results.HasActive && !results.HasResult)
                    {
                        await dialogContext.BeginAsync(_rootDialogName, null, cancellationToken);
                    }

                    // Attempt to save the changes this activity has had on the state
                    if (await _state.TrySaveChangesAsync(turnContext, cancellationToken))
                    {
                        // the save was successful so quite the loop
                        break;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }
    }
}

