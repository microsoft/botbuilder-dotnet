// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// BotStateMemoryScope represents a BotState scoped memory.
    /// </summary>
    /// <remarks>This relies on the BotState object being accessible from turnContext.TurnState.Get&lt;T&gt;().</remarks>
    /// <typeparam name="T">BotState type.</typeparam>
    public class BotStateMemoryScope<T> : MemoryScope
        where T : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotStateMemoryScope{T}"/> class.
        /// </summary>
        /// <param name="name">name of the property.</param>
        public BotStateMemoryScope(string name)
            : base(name, includeInSnapshot: true)
        {
        }

        /// <summary>
        /// Get the backing memory for this scope.
        /// </summary>
        /// <param name="dialogContext">dc.</param>
        /// <returns>memory for the scope.</returns>
        public override object GetMemory(DialogContext dialogContext)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            var botState = GetBotState(dialogContext);
            var cachedState = botState?.GetCachedState(dialogContext.Context);

            return cachedState?.State;
        }

        /// <summary>
        /// Changes the backing object for the memory scope.
        /// </summary>
        /// <param name="dialogContext">dc.</param>
        /// <param name="memory">memory.</param>
        public override void SetMemory(DialogContext dialogContext, object memory)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            var botState = GetBotState(dialogContext);

            if (botState == null)
            {
                throw new ArgumentException($"{typeof(T).Name} is not available.");
            }

            throw new NotSupportedException("You cannot replace the root BotState object");
        }

        /// <summary>
        /// Populates the state cache for this <see cref="BotState"/> from the storage layer.
        /// </summary>
        /// <param name="dialogContext">The dialog context object for this turn.</param>
        /// <param name="force">Optional, <c>true</c> to overwrite any existing state cache;
        /// or <c>false</c> to load state from storage only if the cache doesn't already exist.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task LoadAsync(DialogContext dialogContext, bool force = false, CancellationToken cancellationToken = default)
        {
            var botState = GetBotState(dialogContext);

            if (botState != null)
            {
                await botState.LoadAsync(dialogContext.Context, force, cancellationToken).ConfigureAwait(false);
            }
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
        public override async Task SaveChangesAsync(DialogContext dialogContext, bool force = false, CancellationToken cancellationToken = default)
        {
            var botState = GetBotState(dialogContext);

            if (botState != null)
            {
                await botState.SaveChangesAsync(dialogContext.Context, force, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes any state in storage and the cache for this <see cref="BotState"/>.
        /// </summary>
        /// <param name="dialogContext">The dialog context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task DeleteAsync(DialogContext dialogContext, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private static T GetBotState(DialogContext dialogContext) => dialogContext.Context.TurnState.Get<T>();
    }
}
