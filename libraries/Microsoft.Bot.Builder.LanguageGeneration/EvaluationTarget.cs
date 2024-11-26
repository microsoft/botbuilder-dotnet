// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

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
        public EvaluationTarget(string templateName, IMemory scope)
        {
            TemplateName = templateName;
            Scope = scope;
        }

        /// <summary>
        /// Gets or sets the children template that this template has evaluated and cached currently. 
        /// </summary>
        /// <value>
        /// The children template that this template has evaluated currently. 
        /// </value>
        public Dictionary<string, object> CachedEvaluatedChildren { get; set; } = new Dictionary<string, object>();

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
        public IMemory Scope { get; set; }

        /// <summary>Throws an exception if any of the specified targets equals the specified id such that a loop is detected.</summary>
        /// <param name="targets">The targets to compare.</param>
        /// <param name="id">The id against which the targets should be compared.</param>
        /// <param name="templateName">The template name to include in any resulting exception.</param>
        public static void ThrowIfLoopDetected(
            Stack<EvaluationTarget> targets,
            (string TemplateName, string ScopeVersion) id,
            string templateName)
        {
            foreach (var target in targets)
            {
                if (target.TemplateName == id.TemplateName && target.Scope?.Version() == id.ScopeVersion)
                {
                    throw new InvalidOperationException($"{TemplateErrors.LoopDetected} {string.Join(" => ", targets.Reverse().Select(e => e.TemplateName))} => {templateName}");
                }
            }
        }

        /// <summary>Combines the components of the specified <paramref name="id"/> to create a string key.</summary>
        /// <param name="id">The id retrieved from <see cref="GetId"/>.</param>
        /// <returns>The created string key.</returns>
        public static string CreateKey((string TemplateName, string ScopeVersion) id)
        {
            return id.TemplateName + id.ScopeVersion;
        }

        /// <summary>
        /// Get current instance id. If two target has the same Id,
        /// we can say they have the same template evaluation.
        /// </summary>
        /// <returns>Id.</returns>
        public (string TemplateName, string ScopeVersion) GetId()
        {
            return (TemplateName, Scope?.Version());
        }
    }
}
