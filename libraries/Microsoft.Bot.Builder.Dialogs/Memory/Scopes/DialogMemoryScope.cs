// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// DialogMemoryScope maps "dialog" -> dc.Parent?.ActiveDialog.State ?? ActiveDialog.State
    /// </summary>
    public class DialogMemoryScope : MemoryScope
    {
        public DialogMemoryScope()
            : base(ScopePath.DIALOG)
        {
        }

        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            return dc.Parent?.ActiveDialog?.State ?? dc.ActiveDialog?.State;
        }

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

            // use parent, unless there is no parent, then use ActiveDialog
            var activeDialog = dc.Parent?.ActiveDialog ?? dc.ActiveDialog;
            activeDialog.State = (IDictionary<string, object>)memory;
        }
    }
}
