// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// DialogContextMemoryScope maps "dialogcontext" -> properties.
    /// </summary>
    /// <remarks>
    ///  dc.stack => stack of all dialog ids up to the root dialog.
    ///  dc.activeDialog => id of active dialog.
    ///  dc.parent => id of parent dialog.
    /// </remarks>
    public class DialogContextMemoryScope : MemoryScope
    {
        public DialogContextMemoryScope()
            : base(ScopePath.DialogContext, includeInSnapshot: false)
        {
        }

        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var memory = new JObject();
            JArray stack = new JArray();
            var currentDc = dc;
            
            // go to leaf node
            while (currentDc.Child != null)
            {
                currentDc = currentDc.Child;
            }

            while (currentDc != null)
            {
                // (PORTERS NOTE: javascript stack is reversed with top of stack on end)
                foreach (var item in currentDc.Stack)
                {
                    // filter out ActionScope items because they are internal bookkeeping.
                    if (!item.Id.StartsWith("ActionScope["))
                    {
                        stack.Add(item.Id);
                    }
                }

                currentDc = currentDc.Parent;
            }

            // top of stack is stack[0]. 
            memory["stack"] = stack;
            memory["activeDialog"] = dc.ActiveDialog?.Id;
            memory["parent"] = dc.Parent?.ActiveDialog?.Id;
            return memory;
        }

        public override void SetMemory(DialogContext dc, object memory)
        {
            throw new NotSupportedException("You can't modify the dialogcontext scope");
        }
    }
}
