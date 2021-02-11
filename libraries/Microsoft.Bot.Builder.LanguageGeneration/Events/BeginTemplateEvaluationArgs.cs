// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Provide event data when a certain template is evaluated.
    /// </summary>
    public class BeginTemplateEvaluationArgs : LGEventArgs
    {
        /// <summary>
        /// Gets or sets template name.
        /// </summary>
        /// <value>
        /// Template name.
        /// </value>
        public string TemplateName { get; set; }
    }
}
