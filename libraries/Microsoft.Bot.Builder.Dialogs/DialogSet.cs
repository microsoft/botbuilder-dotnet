// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A related set of dialogs that can all call each other.
    /// </summary>
    public class DialogSet
    {
        private IStatePropertyAccessor<DialogState> _dialogState;
        private IDictionary<string, Dialog> _dialogs;

        public DialogSet(IStatePropertyAccessor<DialogState> dialogState)
        {
            _dialogState = dialogState;
            _dialogs = new Dictionary<string, Dialog>();
        }

        /// <summary>
        /// Adds a new dialog to the set and returns the added dialog.
        /// </summary>
        /// <param name="dialog">The dialog to add.</param>
        /// <returns>The added dialog.</returns>
        public Dialog Add(Dialog dialog)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }

            if (_dialogs.ContainsKey(dialog.Id))
            {
                throw new Exception($"DialogSet.Add(): A dialog with an id of '{dialog.Id}' already added.");
            }

            return _dialogs[dialog.Id] = dialog;
        }

        public async Task<DialogContext> CreateContextAsync(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);

            if (_dialogState == null)
            {
                throw new Exception($"DialogSet.CreateContextAsync(): DialogSet created with a null IStatePropertyAccessor. Must manually factory DialogContext instances in this scenario.");
            }

            // Load/initialize dialog state
            var state = await _dialogState.GetAsync(context, () => { return new DialogState(); }).ConfigureAwait(false);

            // Create and return context
            return new DialogContext(this, context, state);
        }

        /// <summary>
        /// Finds a dialog that was previously added to the set using [add()](#add).
        /// </summary>
        /// <param name="dialogId">ID of the dialog/prompt to lookup.</param>
        /// <returns>dialog if found otherwise null.</returns>
        public Dialog Find(string dialogId)
        {
            if (string.IsNullOrWhiteSpace(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            if (_dialogs.TryGetValue(dialogId, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
