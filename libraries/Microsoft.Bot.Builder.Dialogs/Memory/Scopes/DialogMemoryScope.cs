// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// DialogMemoryScope maps "dialog" -> dc.Parent?.ActiveDialog.State ?? ActiveDialog.State.
    /// </summary>
    public class DialogMemoryScope : MemoryScope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogMemoryScope"/> class.
        /// </summary>
        public DialogMemoryScope()
            : base(ScopePath.Dialog)
        {
        }

        /// <summary>
        /// Gets the backing memory for this scope.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> object for this turn.</param>
        /// <returns>Memory for the scope.</returns>
        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // if active dialog is a container dialog then "dialog" binds to it.
            if (dc.ActiveDialog != null)
            {
                var dialog = dc.FindDialog(dc.ActiveDialog.Id);
                if (dialog is DialogContainer)
                {
                    return dc.ActiveDialog.State;
                }
            }

            // Otherwise we always bind to parent, or if there is no parent the active dialog
            return dc.Parent?.ActiveDialog?.State ?? dc.ActiveDialog?.State;
        }

        /// <summary>
        /// Changes the backing object for the memory scope.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> object for this turn.</param>
        /// <param name="memory">Memory object to set for the scope.</param>
        public override void SetMemory(DialogContext dc, object memory)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (memory == null)
            {
                throw new ArgumentNullException(nameof(memory));
            }

            // if active dialog is a container dialog then "dialog" binds to it
            if (dc.ActiveDialog != null)
            {
                var dialog = dc.FindDialog(dc.ActiveDialog.Id);
                if (dialog is DialogContainer)
                {
                    dc.ActiveDialog.State = (IDictionary<string, object>)memory;
                    return;
                }
            }
            else if (dc.Parent?.ActiveDialog != null)
            {
                dc.Parent.ActiveDialog.State = (IDictionary<string, object>)memory;
                return;
            }
            else if (dc.ActiveDialog != null)
            {
                dc.ActiveDialog.State = (IDictionary<string, object>)memory;
            }

            throw new InvalidOperationException("Cannot set DialogMemoryScope. There is no active dialog or parent dialog in the context");
        }
    }
}
