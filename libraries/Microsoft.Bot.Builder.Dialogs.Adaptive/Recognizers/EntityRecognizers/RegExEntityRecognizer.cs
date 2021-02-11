// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Matches input against a regular expression.
    /// </summary>
    public class RegexEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.RegexEntityRecognizer";

        private string pattern;
        private Regex regex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexEntityRecognizer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public RegexEntityRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Gets or sets the name match result TypeName value.
        /// </summary>
        /// <value>
        /// Name value.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the regular expression pattern value.
        /// </summary>
        /// <value>
        /// Pattern value.
        /// </value>
        [JsonProperty("pattern")]
        public string Pattern
        {
            get
            {
                return this.pattern;
            }

            set
            {
                this.pattern = value;
                this.regex = new Regex(value, RegexOptions.Compiled);
            }
        }

        /// <summary>
        /// Match recognizing implementation.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The matched <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            List<ModelResult> results = new List<ModelResult>();

            var matches = regex.Matches(text);
            if (matches.Count > 0)
            {
                // only if we have a value and the name is not a number "0"
                foreach (var match in matches.Cast<Match>())
                {
                    results.Add(new ModelResult()
                    {
                        TypeName = Name,
                        Text = match.Value,
                        Start = match.Index,
                        End = match.Index + match.Length
                    });
                }
            }

            return results;
        }
    }
}
