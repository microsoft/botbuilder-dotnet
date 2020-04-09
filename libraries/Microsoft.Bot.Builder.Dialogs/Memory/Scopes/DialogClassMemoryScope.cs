// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// DialogMemoryScope maps "class" -> dc.ActiveDialog.Properties.
    /// </summary>
    public class DialogClassMemoryScope : MemoryScope
    {
        public DialogClassMemoryScope()
            : base(ScopePath.DialogClass)
        {
            this.IncludeInSnapshot = false;
        }

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
                if (dialog is DialogContainer)
                {
                    return new ReadOnlyObject(dialog);
                }
            }

            // Otherwise we always bind to parent, or if there is no parent the active dialog
            return new ReadOnlyObject(dc.FindDialog(dc.Parent?.ActiveDialog?.Id ?? dc.ActiveDialog?.Id));
        }

        public override void SetMemory(DialogContext dc, object memory)
        {
            throw new NotSupportedException("You can't modify the class scope");
        }
    }
}
