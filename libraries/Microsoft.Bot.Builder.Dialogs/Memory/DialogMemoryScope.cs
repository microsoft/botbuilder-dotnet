// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Memory;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// DialogMemoryScope doesn't store data in TurnState but instead maps "dialog" -> dc.ActiveDialog.State
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

            return dc.ActiveDialog?.State;
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

            dc.ActiveDialog.State = (IDictionary<string, object>)memory;
        }
    }
}
