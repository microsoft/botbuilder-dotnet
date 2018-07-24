using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class SentencesLenght
    {
        [JsonProperty("srcSentLen")]
        public IEnumerable<int> Source { get; set; }

        [JsonProperty("transSentLen")]
        public IEnumerable<int> Translation { get; set; }
    }
}
