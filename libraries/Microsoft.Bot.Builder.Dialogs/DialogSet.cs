// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A related set of dialogs that can all call each other.
    /// </summary>
    public class DialogSet
    {
        private readonly IStatePropertyAccessor<DialogState> _dialogState;
        private readonly IDictionary<string, Dialog> _dialogs = new Dictionary<string, Dialog>();

        public DialogSet(IStatePropertyAccessor<DialogState> dialogState)
        {
            _dialogState = dialogState ?? throw new ArgumentNullException($"missing {nameof(dialogState)}");
        }

        internal DialogSet()
        {
            // TODO: This is only used by ComponentDialog and future release
            // will refactor to use IStatePropertyAccessor from context
            _dialogState = null;
        }

        /// <summary>
        /// Adds a new dialog to the set and returns the added dialog.
        /// </summary>
        /// <param name="dialog">The dialog to add.</param>
        /// <returns>The DialogSet for fluent calls to Add().</returns>
        public DialogSet Add(Dialog dialog)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }

            if (_dialogs.ContainsKey(dialog.Id))
            {
                throw new ArgumentException($"DialogSet.Add(): A dialog with an id of '{dialog.Id}' already added.");
            }

            _dialogs[dialog.Id] = dialog;
            return this;
        }

        public async Task<DialogContext> CreateContextAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ContextNotNull(turnContext);

            // ToDo: Component Dialog doesn't call this code path. This needs to be cleaned up in 4.1.

            if (_dialogState == null)
            {
                // Note: This shouldn't ever trigger, as the _dialogState is set in the constructor and validated there.
                throw new InvalidOperationException($"DialogSet.CreateContextAsync(): DialogSet created with a null IStatePropertyAccessor.");
            }

            // Load/initialize dialog state
            var state = await _dialogState.GetAsync(turnContext, () => { return new DialogState(); }, cancellationToken).ConfigureAwait(false);

            // Create and return context
            return new DialogContext(this, turnContext, state);
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
