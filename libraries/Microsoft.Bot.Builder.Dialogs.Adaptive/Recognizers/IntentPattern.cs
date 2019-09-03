using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Represents pattern for an intent.
    /// </summary>
    public class IntentPattern
    {
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
        /// The intent.
        /// </summary>
        public string Intent { get; set; }

        /// <summary>
        /// Gets or sets the regex pattern to match
        /// </summary>
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

        public Regex Regex => this.regex;
    }
}
