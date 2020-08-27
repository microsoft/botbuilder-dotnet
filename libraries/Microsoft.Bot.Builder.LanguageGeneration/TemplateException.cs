// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Template Exception that contains diagnostics.
    /// </summary>
    [Serializable]
#pragma warning disable CA1032 // Implement standard exception constructors (by design)
#pragma warning disable CA2229 // Implement serialization constructors (by design)
    public class TemplateException : Exception, ISerializable
#pragma warning restore CA2229 // Implement serialization constructors
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateException"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="diagnostics">List of diagnostics to throw.</param>
        public TemplateException(string message, IList<Diagnostic> diagnostics)
            : base(message)
        {
            Diagnostics = diagnostics;
        }

        /// <summary>
        /// Gets or sets diagnostics.
        /// </summary>
        /// <value>
        /// Diagnostics.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't remove the setter without breaking binary compat)
        public IList<Diagnostic> Diagnostics { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
