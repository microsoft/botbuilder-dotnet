// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Provides helper methods for getting state objects from the turn context.
    /// </summary>
    public static class StateTurnContextExtensions
    {
        /// <summary>
        /// Gets a conversation state property.
        /// </summary>
        /// <typeparam name="TState">The type of the state object to get.</typeparam>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="accessor">The state accessor object for this property.</param>
        /// <returns>The state object.</returns>
        public static async Task<TState> GetStateAsync<TState>(this ITurnContext context, IStatePropertyAccessor<TState> accessor)
            where TState : class, new()
        {
            if (accessor == null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            return await accessor.GetAsync(context).ConfigureAwait(false);
        }

        public static async Task SetStateAsync<TState>(this ITurnContext context, IStatePropertyAccessor<TState> accessor, TState value)
        {
            if (accessor == null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            await accessor.SetAsync(context, value).ConfigureAwait(false);
        }
    }

}
