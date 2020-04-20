// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Runtime template state.
    /// </summary>
    internal class EvaluationTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationTarget"/> class.
        /// </summary>
        /// <param name="templateName">Template name.</param>
        /// <param name="scope">Template scope.</param>
        public EvaluationTarget(string templateName, object scope)
        {
            TemplateName = templateName;
            Scope = scope;
        }

        /// <summary>
        /// Gets or sets the children template that this template has evaluated currently. 
        /// </summary>
        /// <value>
        /// The children template that this template has evaluated currently. 
        /// </value>
        public Dictionary<string, object> EvaluatedChildren { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets template name.
        /// </summary>
        /// <value>
        /// Template name.
        /// </value>
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets scope.
        /// </summary>
        /// <value>
        /// Scope.
        /// </value>
        public object Scope { get; set; }

        /// <summary>
        /// Get current instance id. If two target has the same Id,
        /// we can say they have the same template evaluation.
        /// </summary>
        /// <returns>Id.</returns>
        public string GetId()
        {
            var memory = (CustomizedMemory)Scope;
            var globalMemoryId = memory.GlobalMemory?.Version() ?? string.Empty;
            var localMemoryId = memory.LocalMemory?.ToString() ?? string.Empty;
            return TemplateName + globalMemoryId + localMemoryId;
        }
    }
}
