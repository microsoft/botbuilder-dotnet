// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

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
    /// LG cache scope options.
    /// </summary>
    public enum LGCacheScope
    {
        /// <summary>
        /// Global template cache scope.
        /// </summary>
        Global,

        /// <summary>
        /// Only cache result in the same layer of children in template.
        /// </summary>
        Local,

        /// <summary>
        /// Without cache.
        /// </summary>
        None
    }

    /// <summary>
    /// Options for evaluating LG templates.
    /// </summary>
    public class EvaluationOptions
    {
        private static readonly Regex NullKeyReplaceStrRegex = new Regex(@"\${\s*path\s*}");
        private readonly string _strictModeKey = "@strict";
        private readonly string _replaceNullKey = "@replaceNull";
        private readonly string _lineBreakKey = "@lineBreakStyle";

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationOptions"/> class.
        /// </summary>
        public EvaluationOptions()
        {
            StrictMode = null;
            NullSubstitution = null;
            LineBreakStyle = null;
            Locale = null;
            OnEvent = null;
            CacheScope = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationOptions"/> class.
        /// </summary>
        /// <param name="opt">Instance to copy initial settings from.</param>
        public EvaluationOptions(EvaluationOptions opt)
        {
            StrictMode = opt.StrictMode;
            NullSubstitution = opt.NullSubstitution;
            LineBreakStyle = opt.LineBreakStyle;
            Locale = opt.Locale ?? Thread.CurrentThread.CurrentCulture.Name;
            OnEvent = opt.OnEvent;
            CacheScope = opt.CacheScope;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationOptions"/> class.
        /// </summary>
        /// <param name="optionsList">List of strings containing the options from a LG file.</param>
        public EvaluationOptions(IList<string> optionsList)
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
                        if (key == _strictModeKey)
                        {
                            if (value.ToLowerInvariant() == "true")
                            {
                                StrictMode = true;
                            }
                        }
                        else if (key == _replaceNullKey)
                        {
                            NullSubstitution = (path) => NullKeyReplaceStrRegex.Replace(value, $"{path}");
                        }
                        else if (key == _lineBreakKey)
                        {
                            LineBreakStyle = value.ToLowerInvariant() == LGLineBreakStyle.Markdown.ToString().ToLowerInvariant() ? LGLineBreakStyle.Markdown : LGLineBreakStyle.Default;
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
        /// Gets or sets the locale for evaluating LG.
        /// </summary>
        /// <value>
        /// A CultureInfo or null object.
        /// </value>
        public string Locale { get; set; } = null;

        /// <summary>
        /// Gets or sets the option of a function to replace a null value. If nullSubstitution is specified,
        /// LG evaluator will not throw null exception even the strictMode is on. 
        /// </summary>
        /// <value>
        /// A function.
        /// </value>
        public Func<string, object> NullSubstitution { get; set; } = null;

        /// <summary>
        /// Gets or sets an event handler that handles the emitted events in the evaluation process.
        /// </summary>
        /// <value>
        /// An event handler that handles the emitted events in the evaluation process.
        /// </value>
        public EventHandler OnEvent { get; set; } = null;

        /// <summary>
        /// Gets or sets cache scope of the evaluation result.
        /// </summary>
        /// <value>
        /// Cache scope of the evaluation result.
        /// </value>
        public LGCacheScope? CacheScope { get; set; } = null;

        /// <summary>
        /// Merge a incoming option to current option. If a property in incoming option is not null while it is null in current
        /// option, then the value of this property will be overwritten.
        /// </summary>
        /// <param name="opt">Incoming option for merging.</param>
        /// <returns>Result after merging.</returns>
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
