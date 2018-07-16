// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A related set of dialogs that can all call each other.
    /// </summary>
    public class DialogSet
    {
        private IDictionary<string, IDialog> _dialogs;

        public DialogSet()
        {
            _dialogs = new Dictionary<string, IDialog>();
        }

        /// <summary>
        /// Adds a new dialog to the set and returns the added dialog.
        /// </summary>
        public IDialog Add(string dialogId, IDialog dialog)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            if (dialog == null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }

            if (_dialogs.ContainsKey(dialogId))
            {
                throw new Exception($"DialogSet.add(): A dialog with an id of '{dialogId}' already added.");
            }

            return _dialogs[dialogId] = dialog;
        }

        /// <summary>
        /// Adds a new waterfall to the set and returns the added waterfall.
        /// </summary>
        public Waterfall Add(string dialogId, WaterfallStep[] steps)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            if (steps == null)
            {
                throw new ArgumentNullException(nameof(steps));
            }

            var waterfall = new Waterfall(steps);
            Add(dialogId, waterfall);
            return waterfall;
        }

        public DialogContext CreateContext(ITurnContext context, IDictionary<string, object> state)
        {
            BotAssert.ContextNotNull(context);
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return new DialogContext(this, context, state);
        }

        /// <summary>
        /// Finds a dialog that was previously added to the set using [add()](#add).
        /// </summary>
        /// <param name="dialogId">ID of the dialog/prompt to lookup.</param>
        /// <returns>dialog if found otherwise null.</returns>
        public IDialog Find(string dialogId)
        {
            if (string.IsNullOrEmpty(dialogId))
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
