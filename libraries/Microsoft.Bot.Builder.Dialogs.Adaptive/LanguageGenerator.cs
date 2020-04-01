// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        /// <param name="turnContext">turnContext.</param>
        /// <param name="template">template or [templateId].</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>text.</returns>
        public abstract Task<string> Generate(ITurnContext turnContext, string template, object data);
    }
}
