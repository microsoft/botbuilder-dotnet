using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class RegexEntityRecognizer : TextEntityRecognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.RegexEntityRecognizer";

        private string pattern;
        private Regex regex;

        public RegexEntityRecognizer()
        {
        }

        [JsonProperty("name")]
        public string Name { get; set; }

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
