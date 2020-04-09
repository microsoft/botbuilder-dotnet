// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
                throw new ArgumentNullException($"{nameof(dialogContext)} is null");
            }

            var cachedState = dialogContext.Context.TurnState.Get<object>(typeof(T).Name);
            if (cachedState != null)
            {
                return cachedState.GetType().GetProperty("State").GetValue(cachedState);
            }

            // this memory scope is not available
            return null;
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
                throw new ArgumentNullException($"{nameof(dialogContext)} is null");
            }

            var botState = dialogContext.Context.TurnState.Get<T>();
            if (botState == null)
            {
                throw new ArgumentException($"{typeof(T).Name} is not available.");
            }

            throw new NotSupportedException("You cannot replace the root BotState object");
        }

        public override async Task LoadAsync(DialogContext dialogContext, bool force = false, CancellationToken cancellationToken = default)
        {
            var botState = dialogContext.Context.TurnState.Get<T>();
            if (botState != null)
            {
                await botState.LoadAsync(dialogContext.Context, force, cancellationToken).ConfigureAwait(false);
            }
        }

        public override async Task SaveChangesAsync(DialogContext dialogContext, bool force = false, CancellationToken cancellationToken = default)
        {
            var botState = dialogContext.Context.TurnState.Get<T>();
            if (botState != null)
            {
                await botState.SaveChangesAsync(dialogContext.Context, force, cancellationToken).ConfigureAwait(false);
            }
        }

        public override Task DeleteAsync(DialogContext dialogContext, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
