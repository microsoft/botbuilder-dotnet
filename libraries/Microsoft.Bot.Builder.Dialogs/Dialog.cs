// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Base class for controls.
    /// </summary>
    public abstract class Dialog
    {
        /// <summary>
        /// Starts the dialog. Depending on the dialog, its possible for the dialog to finish
        /// immediately so it's advised to check the completion object returned by `begin()` and ensure
        /// that the dialog is still active before continuing.
        /// </summary>
        /// <param name="context">Context for the current turn of the conversation with the user.</param>
        /// <param name="state">A state object that the dialog will use to persist its current state. This should be an empty object which the dialog will populate. The bot should persist this with its other conversation state for as long as the dialog is still active.</param>
        /// <param name="options">(Optional) additional options supported by the dialog.</param>
        /// <returns>DialogCompletion result.</returns>
        public async Task<DialogCompletion> BeginAsync(ITurnContext context, IDictionary<string, object> state, IDictionary<string, object> options = null)
        {
            BotAssert.ContextNotNull(context);
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            // Create empty dialog set and ourselves to it
            var dialogs = new DialogSet();
            dialogs.Add("dialog", (IDialog)this);

            // Start the control
            IDictionary<string, object> result = null;
            var dc = new DialogContext(dialogs, context, state, (r) => { result = r; });

            await dc.BeginAsync("dialog", options);
            return dc.ActiveDialog != null
                    ?
                new DialogCompletion { IsActive = true, IsCompleted = false }
                    :
                new DialogCompletion { IsActive = false, IsCompleted = true, Result = result };
        }

        /// <summary>
        /// Passes a users reply to the dialog for further processing.The bot should keep calling
        /// 'continue()' for future turns until the dialog returns a completion object with
        /// 'isCompleted == true'. To cancel or interrupt the prompt simply delete the `state` object
        /// being persisted.
        /// </summary>
        /// <param name="context">Context for the current turn of the conversation with the user.</param>
        /// <param name="state">A state object that was previously initialized by a call to [begin()](#begin).</param>
        /// <returns>DialogCompletion result.</returns>
        public async Task<DialogCompletion> ContinueAsync(ITurnContext context, IDictionary<string, object> state)
        {
            BotAssert.ContextNotNull(context);
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            // Create empty dialog set and ourselves to it
            var dialogs = new DialogSet();
            dialogs.Add("dialog", (IDialog)this);

            // Continue the dialog
            IDictionary<string, object> result = null;
            var dc = new DialogContext(dialogs, context, state, (r) => { result = r; });
            if (dc.ActiveDialog != null)
            {
                await dc.ContinueAsync();
                return dc.ActiveDialog != null
                        ?
                    new DialogCompletion { IsActive = true, IsCompleted = false }
                        :
                    new DialogCompletion { IsActive = false, IsCompleted = true, Result = result };
            }
            else
            {
                return new DialogCompletion { IsActive = false, IsCompleted = false };
            }
        }
    }
}
