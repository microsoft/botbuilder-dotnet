// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// DialogMemoryScope maps "class" -> dc.ActiveDialog.Properties
    /// </summary>
    public class ClassMemoryScope : MemoryScope
    {
        public ClassMemoryScope()
            : base(ScopePath.CLASS)
        {
            this.IsReadOnly = true;
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
                if (dialog != null)
                {
                    return dialog;
                }
            }

            return null;
        }

        public override void SetMemory(DialogContext dc, object memory)
        {
            throw new NotSupportedException();
        }
    }
}
