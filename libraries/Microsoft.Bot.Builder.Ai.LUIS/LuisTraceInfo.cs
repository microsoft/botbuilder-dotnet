// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Cognitive.LUIS;
using Microsoft.Cognitive.LUIS.Models;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{

    /// <summary>
    /// This class represents all the trace info that we collect from the LUIS Recognizer Middleware
    /// </summary>
    public class LuisTraceInfo
    {
        [JsonProperty("recognizerResult")]
        public RecognizerResult RecognizerResult { set; get; }

        [JsonProperty("luisResult")]
        public LuisResult LuisResult { set; get; }

        [JsonProperty("luisModel")]
        public ILuisModel LuisModel { set; get; }

        [JsonProperty("luisOptions")]
        public ILuisOptions LuisOptions { set; get; }
    }
}
