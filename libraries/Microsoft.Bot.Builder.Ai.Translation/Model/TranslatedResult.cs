using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class TranslatedResult
    {
        [JsonProperty("translations")]
        public IEnumerable<TranslationModel> Translations { get; set; }
    }
}
