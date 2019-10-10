// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Defines how to generate an IActivity based on all of the parameters which drive resolution.
    /// </summary>
    /// <typeparam name="T">type of IActivity to return. </typeparam>
    public interface IActivityGenerator
    {
        /// <summary>
        /// Generate a IActivity based on paramters.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="template">template or [templateId].</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
        Task<Activity> Generate(ITurnContext turnContext, string template, object data);
    }
}
