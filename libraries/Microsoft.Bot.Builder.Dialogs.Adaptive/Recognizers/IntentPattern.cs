// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Represents pattern for an intent.
    /// </summary>
    public class IntentPattern
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.IntentPattern";

        private Regex regex;
        private string pattern;

        public IntentPattern()
        {
        }

        public IntentPattern(string intent, string pattern)
        {
            this.Intent = intent;
            this.Pattern = pattern;
        }

        /// <summary>
        /// Gets or sets the intent.
        /// </summary>
        /// <value>
        /// The intent.
        /// </value>
        [JsonProperty("intent")]
        public string Intent { get; set; }

        /// <summary>
        /// Gets or sets the regex pattern to match.
        /// </summary>
        /// <value>
        /// The regex pattern to match.
        /// </value>
        [JsonProperty("pattern")]
        public string Pattern
        {
            get
            {
                return pattern;
            }

            set
            {
                this.pattern = value;
                this.regex = new Regex(pattern, RegexOptions.Compiled);
            }
        }

        [JsonIgnore]
        public Regex Regex => this.regex;
    }
}
