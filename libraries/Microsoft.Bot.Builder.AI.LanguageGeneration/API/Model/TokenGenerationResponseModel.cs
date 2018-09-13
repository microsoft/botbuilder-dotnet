using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API.Model
{
    internal class TokenGenerationResponseModel
    {
        [JsonProperty("text")]
        public string Token { get; set; }
    }
}
