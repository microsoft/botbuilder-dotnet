// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Interface which defines methods for how you can get data from a property source such as BotState.
    /// </summary>
    /// <typeparam name="T">type of the property.</typeparam>
    public interface IStatePropertyAccessor<T> : IStatePropertyInfo
    {
        /// <summary>
        /// Get the property value from the source.
        /// </summary>
        /// <param name="turnContext">Turn Context.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<T> GetAsync(ITurnContext turnContext);

        /// <summary>
        /// Delete the property from the source.
        /// </summary>
        /// <param name="turnContext">Turn Context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteAsync(ITurnContext turnContext);

        /// <summary>
        /// Set the property value on the source.
        /// </summary>
        /// <param name="turnContext">Turn Context.</param>
        /// <param name="value">the value to set</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetAsync(ITurnContext turnContext, T value);
    }
}
