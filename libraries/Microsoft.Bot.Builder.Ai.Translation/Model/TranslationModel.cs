using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class TranslationModel
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("alignment")]
        public Alignment Alignment { get; set; }

        [JsonProperty("sentLen")]
        public SentencesLenght SentencesLenght { get; set; }
    }
}
