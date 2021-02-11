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
        private Regex regex;
        private string pattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntentPattern"/> class.
        /// </summary>
        public IntentPattern()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntentPattern"/> class.
        /// </summary>
        /// <param name="intent">The intent.</param>
        /// <param name="pattern">The regex pattern to match.</param>
        public IntentPattern(string intent, string pattern)
        {
            Intent = intent;
            Pattern = pattern;
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
                this.regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        /// <summary>
        /// Gets the <see cref="Regex"/> instance.
        /// </summary>
        /// <value>
        /// The <see cref="Regex"/> instance.
        /// </value>
        [JsonIgnore]
        public Regex Regex => this.regex;
    }
}
