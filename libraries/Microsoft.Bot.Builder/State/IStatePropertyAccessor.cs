// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Interface which defines methods for how you can get data from a property source,
    /// such as <see cref="BotState"/>.
    /// </summary>
    /// <typeparam name="T">type of the property.</typeparam>
    public interface IStatePropertyAccessor<T> : IStatePropertyInfo
    {
        /// <summary>
        /// Gets the property value from the source.
        /// </summary>
        /// <param name="turnContext">Turn Context.</param>
        /// <param name="defaultValueFactory">Function which defines the property value to be returned if no value has been set.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<T> GetAsync(ITurnContext turnContext, Func<T> defaultValueFactory = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Delete the property from the source.
        /// </summary>
        /// <param name="turnContext">Turn Context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Set the property value on the source.
        /// </summary>
        /// <param name="turnContext">Turn Context.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetAsync(ITurnContext turnContext, T value, CancellationToken cancellationToken = default(CancellationToken));
    }
}
