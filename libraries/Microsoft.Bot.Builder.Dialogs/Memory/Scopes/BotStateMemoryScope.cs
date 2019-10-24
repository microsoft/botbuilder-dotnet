// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// BotStateMemoryScope represents a BotState scoped memory.
    /// </summary>
    /// <remarks>This relies on the BotState object being accessible from turnContext.TurnState.Get&lt;T&gt().</remarks>
    /// <typeparam name="T">botState type.</typeparam>
    public class BotStateMemoryScope<T> : MemoryScope
        where T : BotState
    {
        public BotStateMemoryScope(string name)
            : base(name, false)
        {
        }

        /// <summary>
        /// Get the backing memory for this scope.
        /// </summary>
        /// <param name="dc">dc.</param>
        /// <returns>memory for the scope.</returns>
        public override object GetMemory(DialogContext dc)
        {
            var property = GetBotStateProperty(dc);
            
            // NOTE: the BotState should already be in memory, this is accessing the in-memory cache, which means it should complete without blocking
            return property.GetAsync(dc.Context, () => new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Changes the backing object for the memory scope.
        /// </summary>
        /// <param name="dc">dc.</param>
        /// <param name="memory">memory.</param>
        public override void SetMemory(DialogContext dc, object memory)
        {
            var property = GetBotStateProperty(dc);

            // NOTE: the BotState should already be in memory, this is accessing the in-memory cache, which means it should complete without blocking
            property.SetAsync(dc.Context, memory).GetAwaiter().GetResult();
        }

        private IStatePropertyAccessor<object> GetBotStateProperty(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var conversationState = dc.Context.TurnState.Get<T>();
            if (conversationState == null)
            {
                throw new ArgumentNullException($"There is no {typeof(T).Name} registered in TurnContext.TurnState.Get<{typeof(T).Name}>().  Have you registered {typeof(T).Name}?");
            }

            return conversationState.CreateProperty<object>(this.GetType().Name);
        }
    }
}
