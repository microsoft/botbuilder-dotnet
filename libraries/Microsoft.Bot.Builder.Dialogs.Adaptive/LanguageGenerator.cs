// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Defines interface for a Language Generator system to bind to text.
    /// </summary>
    public abstract class LanguageGenerator
    {
        /// <summary>
        /// Method to bind data to string.
        /// </summary>
        /// <param name="dialogContext">dialogContext.</param>
        /// <param name="template">template or [templateId].</param>
        /// <param name="data">data to bind to.</param>
        /// <param name="cancellationToken">the <see cref="CancellationToken"/> for the task.</param>
        /// <returns>object or text.</returns>
        public abstract Task<object> GenerateAsync(DialogContext dialogContext, string template, object data, CancellationToken cancellationToken = default);
    }
}
