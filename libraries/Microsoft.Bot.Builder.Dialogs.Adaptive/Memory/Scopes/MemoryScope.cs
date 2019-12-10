// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// MemoryScope represents a named memory scope abstract class.
    /// </summary>
    public abstract class MemoryScope
    {
        public MemoryScope(string name, bool includeInSnapshot = true)
        {
            this.IncludeInSnapshot = includeInSnapshot;
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

        /// <summary>
        /// Populates the state cache for this <see cref="BotState"/> from the storage layer.
        /// </summary>
        /// <param name="dialogContext">The dialog context object for this turn.</param>
        /// <param name="force">Optional, <c>true</c> to overwrite any existing state cache;
        /// or <c>false</c> to load state from storage only if the cache doesn't already exist.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual Task LoadAsync(DialogContext dialogContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Writes the state cache for this <see cref="BotState"/> to the storage layer.
        /// </summary>
        /// <param name="dialogContext">The dialog context object for this turn.</param>
        /// <param name="force">Optional, <c>true</c> to save the state cache to storage;
        /// or <c>false</c> to save state to storage only if a property in the cache has changed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual Task SaveChangesAsync(DialogContext dialogContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes any state in storage and the cache for this <see cref="BotState"/>.
        /// </summary>
        /// <param name="dialogContext">The dialog context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual Task DeleteAsync(DialogContext dialogContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}
