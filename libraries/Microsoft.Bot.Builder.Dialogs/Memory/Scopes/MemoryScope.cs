// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// MemoryScope represents a named memory scope abstract class.
    /// </summary>
    public abstract class MemoryScope
    {
        public MemoryScope(string name)
        {
            this.IncludeInSnapshot = true;
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets name of the scope.
        /// </summary>
        /// <value>
        /// Name of the scope.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this memory should be included in snapshot.
        /// </summary>
        /// <value>
        /// True or false.
        /// </value>
        public bool IncludeInSnapshot { get; protected set; }

        /// <summary>
        /// Get the backing memory for this scope.
        /// </summary>
        /// <param name="dc">dc.</param>
        /// <returns>memory for the scope.</returns>
        public abstract object GetMemory(DialogContext dc);

        /// <summary>
        /// Changes the backing object for the memory scope.
        /// </summary>
        /// <param name="dc">dc.</param>
        /// <param name="memory">memory.</param>
        public abstract void SetMemory(DialogContext dc, object memory);
    }
}
