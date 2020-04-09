// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// DialogMemoryScope maps "this" -> dc.ActiveDialog.State.
    /// </summary>
    public class ThisMemoryScope : MemoryScope
    {
        public ThisMemoryScope()
            : base(ScopePath.This)
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
