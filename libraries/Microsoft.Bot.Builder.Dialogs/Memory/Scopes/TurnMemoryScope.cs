// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// TurnMemoryScope represents memory scoped to the current turn.
    /// </summary>
    public class TurnMemoryScope : MemoryScope
    {
        public TurnMemoryScope()
            : base(ScopePath.Turn)
        {
        }

        /// <summary>
        /// Get the backing memory for this scope.
        /// </summary>
        /// <param name="dc">dc.</param>
        /// <returns>memory for the scope.</returns>
        public override object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (!dc.Context.TurnState.TryGetValue(ScopePath.Turn, out object val))
            {
                val = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                dc.Context.TurnState[ScopePath.Turn] = val;
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

            dc.Context.TurnState[ScopePath.Turn] = memory;
        }
    }
}
