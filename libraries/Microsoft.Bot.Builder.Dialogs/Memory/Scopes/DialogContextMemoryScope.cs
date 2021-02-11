// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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
        /// <summary>
        /// Stack name.
        /// </summary>
        public const string Stack = "stack";

        /// <summary>
        /// Active dialog name.
        /// </summary>
        public const string ActiveDialog = "activeDialog";

        /// <summary>
        /// Parent name.
        /// </summary>
        public const string Parent = "parent";

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContextMemoryScope"/> class.
        /// </summary>
        public DialogContextMemoryScope()
            : base(ScopePath.DialogContext, includeInSnapshot: false)
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

            var memory = new JObject();
            var stack = new JArray();
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
                    if (!item.Id.StartsWith("ActionScope[", StringComparison.Ordinal))
                    {
                        stack.Add(item.Id);
                    }
                }

                currentDc = currentDc.Parent;
            }

            // top of stack is stack[0]. 
            memory[Stack] = stack;
            memory[ActiveDialog] = dc.ActiveDialog?.Id;
            memory[Parent] = dc.Parent?.ActiveDialog?.Id;
            return memory;
        }

        /// <summary>
        /// Changes the backing object for the memory scope.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> object for this turn.</param>
        /// <param name="memory">Memory object to set for the scope.</param>
        /// <remarks>Method not supported. You can't modify the dialogcontext scope.</remarks>
        public override void SetMemory(DialogContext dc, object memory)
        {
            throw new NotSupportedException("You can't modify the dialogcontext scope");
        }
    }
}
