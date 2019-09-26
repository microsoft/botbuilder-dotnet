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

            // if active dialog is a container dialog then "dialog" binds to it
            if (dc.ActiveDialog != null)
            {
                var dialog = dc.FindDialog(dc.ActiveDialog.Id);
                if (dialog is DialogContainer)
                {
                    return dc.ActiveDialog.State;
                }
            }

            // Otherwise we always bind to parent 
            return dc.Parent?.ActiveDialog?.State;
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

            throw new Exception("Cannot set DialogMemoryScope. There is no active dialog container dialog or parent dialog in the context");
        }
    }
}
