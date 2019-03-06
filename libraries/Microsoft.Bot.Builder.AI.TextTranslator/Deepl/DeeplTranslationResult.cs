using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.TextTranslator.Deepl
{
    internal class DeeplTranslationResult
    {
        [JsonProperty("detected_source_language")]
        public string DetectedSourceLanguage { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
