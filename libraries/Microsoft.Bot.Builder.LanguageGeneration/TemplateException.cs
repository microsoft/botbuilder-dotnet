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
    public class TemplateException : Exception, ISerializable
    {
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
        public IList<Diagnostic> Diagnostics { get; set; }
    }
}
