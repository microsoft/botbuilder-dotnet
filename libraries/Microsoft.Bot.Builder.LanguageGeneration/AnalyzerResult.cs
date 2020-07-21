// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Analyzer result. Contains variables and template references.
    /// </summary>
    public class AnalyzerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzerResult"/> class.
        /// </summary>
        /// <param name="variables">Init varibales.</param>
        /// <param name="templateReferences">Init template references.</param>
        public AnalyzerResult(List<string> variables = null, List<string> templateReferences = null)
        {
            this.Variables = (variables ?? new List<string>()).Distinct().ToList();
            this.TemplateReferences = (templateReferences ?? new List<string>()).Distinct().ToList();
        }

        /// <summary>
        /// Gets or sets variables that this template contains.
        /// </summary>
        /// <value>
        /// Variables that this template contains.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't remove the setter without breaking binary compat)
        public List<string> Variables { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets template references that this template contains.
        /// </summary>
        /// <value>
        /// Template references that this template contains.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't remove the setter without breaking binary compat)
        public List<string> TemplateReferences { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Combine two analyzer results.
        /// </summary>
        /// <param name="outputItem">Another analyzer result.</param>
        /// <returns>Combined analyzer result.</returns>
        public AnalyzerResult Union(AnalyzerResult outputItem)
        {
            this.Variables = this.Variables.Union(outputItem.Variables).ToList();
            this.TemplateReferences = this.TemplateReferences.Union(outputItem.TemplateReferences).ToList();
            return this;
        }
    }
}
