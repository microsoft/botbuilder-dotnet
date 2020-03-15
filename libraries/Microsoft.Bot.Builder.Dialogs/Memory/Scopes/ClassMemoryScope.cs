// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// DialogMemoryScope maps "class" -> dc.ActiveDialog.Properties.
    /// </summary>
    public class ClassMemoryScope : MemoryScope
    {
        public ClassMemoryScope()
            : base(ScopePath.Class)
        {
            this.IncludeInSnapshot = false;
        }

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
                if (dialog != null)
                {
                    return new ReadOnlyObject(dialog);
                }
            }

            return null;
        }

        public override void SetMemory(DialogContext dc, object memory)
        {
            throw new NotSupportedException("You can't modify the class scope");
        }
    }
}
