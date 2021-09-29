// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Options for analyzing LG templates.
    /// </summary>
    public class AnalyzerOptions
    {
        private readonly string _throwOnRecursive = "@throwOnRecursive";

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzerOptions"/> class.
        /// </summary>
        public AnalyzerOptions()
        {
            ThrowOnRecursive = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzerOptions"/> class.
        /// </summary>
        /// <param name="opt">Instance to copy analyzer settings from.</param>
        public AnalyzerOptions(AnalyzerOptions opt)
        {
            ThrowOnRecursive = opt.ThrowOnRecursive;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzerOptions"/> class.
        /// </summary>
        /// <param name="optionsList">List of strings containing the options from an LG file.</param>
        public AnalyzerOptions(IList<string> optionsList)
        {
            if (optionsList != null)
            {
                foreach (var option in optionsList)
                {
                    if (!string.IsNullOrWhiteSpace(option) && option.Contains("="))
                    {
                        var index = option.IndexOf('=');
                        var key = option.Substring(0, index).Trim();
                        var value = option.Substring(index + 1).Trim();
                        
                        if (key.Equals(_throwOnRecursive, StringComparison.OrdinalIgnoreCase))
                        {
                            if (value.ToLowerInvariant() == "true")
                            {
                                ThrowOnRecursive = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value determining if recursive calls throw an exception.
        /// </summary>
        /// <value>
        /// When true, throw an exception if a recursive call is detected.
        /// </value>
        public bool? ThrowOnRecursive { get; set; } = null;

        /// <summary>
        /// Merge a incoming option to current option. If a property in incoming option is not null while it is null in current
        /// option, then the value of this property will be overwritten.
        /// </summary>
        /// <param name="opt">Incoming option for merging.</param>
        /// <returns>Result after merging.</returns>
        public AnalyzerOptions Merge(AnalyzerOptions opt)
        {
            var properties = typeof(AnalyzerOptions).GetProperties();
            foreach (var property in properties)
            {
                if (property.GetValue(this) == null && property.GetValue(opt) != null)
                {
                    property.SetValue(this, property.GetValue(opt));
                }
            }

            return this;
        }
    }
}
