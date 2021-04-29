// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// DialogMemoryScope maps "lg" -> dc.Parent?.ActiveDialog.State ?? ActiveDialog.State.
    /// </summary>
    public class LGMemoryScope : MemoryScope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LGMemoryScope"/> class.
        /// </summary>
        public LGMemoryScope()
            : base(ScopePath.LG, false)
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

            if (!dc.Context.TurnState.TryGetValue(ScopePath.LG, out object val))
            {
                val = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                dc.Context.TurnState[ScopePath.LG] = val;
            }

            return val;
        }

        /// <summary>
        /// Changes the backing object for the memory scope.
        /// </summary>
        /// <param name="dc">dc.</param>
        /// <param name="memory">memory.</param>
        public override void SetMemory(DialogContext dc, object memory)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            dc.Context.TurnState[ScopePath.LG] = memory;
        }
    }
}
