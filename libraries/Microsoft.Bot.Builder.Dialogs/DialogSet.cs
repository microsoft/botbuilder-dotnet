// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A related set of dialogs that can all call each other.
    /// </summary>
    /// <typeparam name="C">TurnContext</typeparam>
    public class DialogSet
    {
        private IDictionary<string, Dialog> _dialogs;

        public DialogSet()
        {
            _dialogs = new Dictionary<string, Dialog>();
        }

        /// <summary>
        /// Adds a new dialog to the set and returns the added dialog.
        /// </summary>
        public Dialog Add(string dialogId, Dialog dialog)
        {
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
            var waterfall = new Waterfall(steps);
            Add(dialogId, waterfall);
            return waterfall;
        }

        public DialogContext CreateContext(ITurnContext context, object state)
        {
            var d = (IDictionary<string, object>)state;
            object value;
            if (!d.TryGetValue("dialogStack", out value))
            {
                value = new Stack<DialogInstance>();
                d["dialogStack"] = value;
            }
            return new DialogContext(this, context, (Stack<DialogInstance>)value);
        }

        /// <summary>
        /// Finds a dialog that was previously added to the set using [add()](#add).
        /// </summary>
        /// <param name="dialogId">ID of the dialog/prompt to lookup.</param>
        /// <returns>dialog if found otherwise null</returns>
        public Dialog Find(string dialogId)
        {
            Dialog result;
            if (_dialogs.TryGetValue(dialogId, out result))
            {
                return result;
            }
            return null;
        }
    }
}
