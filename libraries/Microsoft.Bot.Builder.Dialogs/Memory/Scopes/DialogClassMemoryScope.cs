// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// DialogMemoryScope maps "dialogclass" -> dc.Parent.ActiveDialog Properties.
    /// </summary>
    public class DialogClassMemoryScope : MemoryScope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogClassMemoryScope"/> class.
        /// </summary>
        public DialogClassMemoryScope()
            : base(ScopePath.DialogClass)
        {
            this.IncludeInSnapshot = false;
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

            // if active dialog is a container dialog then "dialogclass" binds to it.
            if (dc.ActiveDialog != null)
            {
                var dialog = dc.FindDialog(dc.ActiveDialog.Id);
                if (dialog is DialogContainer container && !container.IgnoreMemoryScopeBinding)
                {
                    return new ReadOnlyObject(dialog);
                }
            }

            // Otherwise we always bind to parent, or if there is no parent the active dialog
            return new ReadOnlyObject(dc.FindDialog(dc.Parent?.ActiveDialog?.Id ?? dc.ActiveDialog?.Id));
        }

        /// <summary>
        /// Changes the backing object for the memory scope.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> object for this turn.</param>
        /// <param name="memory">Memory object to set for the scope.</param>
        /// <remarks>Method not supported. You can't modify the dialogclass scope.</remarks>
        public override void SetMemory(DialogContext dc, object memory)
        {
            throw new NotSupportedException("You can't modify the dialogclass scope");
        }
    }
}
