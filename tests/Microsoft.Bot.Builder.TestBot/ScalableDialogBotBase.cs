// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.TestBot
{
    /// <summary>
    /// This implemnentation can be used as the basis for a scalable bot which has multiple instances running on
    /// multiple nodes. The assumption there being the try-save operation is guarded with an eTag condition. When the
    /// eTag condition fails the load and business logic should be repeated before the save is attempted again.
    /// Because the BotState provider uses a shared cache the semaphore is still required in this code. As a result this
    /// code behaves correctly but is inefficient. There are a couple of solutions: reduce the granularity of the locking
    /// to make it specific to a conversation or use caching that is created and used inside the scope of the load/try-save.
    /// </summary>
    public class ScalableDialogBotBase : IBot
    {
        private SemaphoreSlim _semaphore;
        private IBotStoreManager _storeManager;
        private string _rootDialogName;

        public ScalableDialogBotBase(TestBotAccessors accessors, string rootDialogName)
        {
            // create the DialogSet from accessor
            Dialogs = new DialogSet(accessors.ConversationDialogState);

            // a semaphore to serialize access to the bot state
            _semaphore = accessors.SemaphoreSlim;

            // the state to attempt to save at the end of every turn
            _storeManager = accessors.StoreManager;

            // the name of the root dialog this bot runs
            _rootDialogName = rootDialogName;
        }

        protected DialogSet Dialogs { get; private set; }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // The loop here is the classic eTag retry loop. After a save failure you need to start over
            // with a re-load of the state. The assumption being you have implemented these abstractions
            // against a store such as Azure Blob Storage that supports eTag conditions on save.
            while (true)
            {
                // We only want to pump one activity at a time through the state.
                // Note the state is shared across all instances of this IBot class so we
                // create the semaphore globally with the accessors.

                // The semaphore is only necessary because we are using the BotState which has a
                // process wide singleton cache. If this cache could be maintained between the load and save
                // the semaphore would not be necessary.
                try
                {
                    await _semaphore.WaitAsync();

                    cancellationToken.ThrowIfCancellationRequested();

                    // explicit load of the bot state
                    await _storeManager.LoadAsync(turnContext, cancellationToken);

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
                    var saveSuccess = await _storeManager.TrySaveChangesAsync(turnContext, cancellationToken);
                    if (saveSuccess)
                    {
                        // the save was successful so quit the loop
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

