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

        /// <summary>
        /// Gets or sets the option of rendering new line characters.
        /// </summary>
        /// <value>
        /// if lineBreakStyle is 'markdown', a new line character will be replaced by two new line characters. Otherwise, it will be the same.
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

        public static EvaluationOptions ExtractOptionsFromStringArray(IList<string> options)
        {
            var opt = new EvaluationOptions();
            if (options == null)
            {
                return opt;
            }

            var strictModeKey = "@strict";
            var replaceNullKey = "@replaceNull";
            var lineBreakKey = "@lineBreakStyle";
            foreach (var option in options)
            {
                if (!string.IsNullOrWhiteSpace(option) && option.Contains("="))
                {
                    var index = option.IndexOf('=');
                    var key = option.Substring(0, index).Trim();
                    var value = option.Substring(index + 1).Trim().ToLower();
                    if (key == strictModeKey)
                    {
                        if (value == "true")
                        {
                            opt.StrictMode = true;
                        }
                    }
                    else if (key == replaceNullKey)
                    {
                        opt.NullSubstitution = (path) => NullKeyReplaceStrRegex.Replace(value, $"{path}");
                    }
                    else if (key == lineBreakKey)
                    {
                        opt.LineBreakStyle = value.ToLower() == LGLineBreakStyle.Markdown.ToString().ToLower() ? LGLineBreakStyle.Markdown : LGLineBreakStyle.Default;
                    }
                }
            }

            return opt;
        }

        /// <summary>
        /// Merge a incoming option to current option. If a property in incoming option is not null while it is null in current
        /// option, then the value of this property will be overwritten.
        /// </summary>
        /// <param name="opt">the incoming option for merging.</param>
        public void MergeOptions(EvaluationOptions opt)
        {
            var properties = typeof(EvaluationOptions).GetProperties();
            foreach (var property in properties)
            {
                if (property.GetValue(this) == null && property.GetValue(opt) != null)
                {
                    property.SetValue(this, property.GetValue(opt));
                }
            }
        }
    }
}
