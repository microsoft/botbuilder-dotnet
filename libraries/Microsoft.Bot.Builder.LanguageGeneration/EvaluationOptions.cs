// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Options for LG rendering the line break in text.
    /// </summary>
    public enum LGLineBreakStyle
    {
        /// <summary>
        /// Default mode.
        /// </summary>
        Default,

        /// <summary>
        /// Markdown mode.
        /// </summary>
        Markdown,
    }

    /// <summary>
    /// Options for evaluation of LG template <see cref="EvaluationOptions"/> class.
    /// </summary>
    public class EvaluationOptions
    {
        private static readonly Regex NullKeyReplaceStrRegex = new Regex(@"\${\s*path\s*}");
        private readonly string strictModeKey = "@strict";
        private readonly string replaceNullKey = "@replaceNull";
        private readonly string lineBreakKey = "@lineBreakStyle";

        public EvaluationOptions()
        {
            this.StrictMode = null;
            this.NullSubstitution = null;
            this.LineBreakStyle = null;
        }

        public EvaluationOptions(EvaluationOptions opt)
        {
            this.StrictMode = opt.StrictMode;
            this.NullSubstitution = opt.NullSubstitution;
            this.LineBreakStyle = opt.LineBreakStyle;
        }

        public EvaluationOptions(IList<string> optionStrList)
        {
            if (optionStrList != null)
            {
                foreach (var optionStr in optionStrList)
                {
                    if (!string.IsNullOrWhiteSpace(optionStr) && optionStr.Contains("="))
                    {
                        var index = optionStr.IndexOf('=');
                        var key = optionStr.Substring(0, index).Trim();
                        var value = optionStr.Substring(index + 1).Trim();
                        if (key == strictModeKey)
                        {
                            if (value.ToLower() == "true")
                            {
                                StrictMode = true;
                            }
                        }
                        else if (key == replaceNullKey)
                        {
                            NullSubstitution = (path) => NullKeyReplaceStrRegex.Replace(value, $"{path}");
                        }
                        else if (key == lineBreakKey)
                        {
                            LineBreakStyle = value.ToLower() == LGLineBreakStyle.Markdown.ToString().ToLower() ? LGLineBreakStyle.Markdown : LGLineBreakStyle.Default;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the option of rendering new line characters.
        /// </summary>
        /// <value>
        /// A string value represents the line break style in LG.
        /// </value>
        public LGLineBreakStyle? LineBreakStyle { get; set; } = null;

        /// <summary>
        /// Gets or sets the option of whether throwing an error when evaluating a null reference.
        /// </summary>
        /// <value>
        /// A boolean or null value.
        /// </value>
        public bool? StrictMode { get; set; } = null;

        /// <summary>
        /// Gets or sets the option of a function to replace a null value. If nullSubstitution is specified,
        /// LG evaluator will not throw null exception even the strictMode is on. 
        /// </summary>
        /// <value>
        /// A function.
        /// </value>
        public Func<string, object> NullSubstitution { get; set; } = null;

        /// <summary>
        /// Merge a incoming option to current option. If a property in incoming option is not null while it is null in current
        /// option, then the value of this property will be overwritten.
        /// </summary>
        /// <param name="opt">The incoming option for merging.</param>
        /// <returns>The result after merging.</returns>
        public EvaluationOptions Merge(EvaluationOptions opt)
        {
            var properties = typeof(EvaluationOptions).GetProperties();
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
