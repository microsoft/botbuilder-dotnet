// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Luis
{

    /// <summary>
    /// This class represents all the trace info that we collect from the LUIS Recognizer Middleware
    /// </summary>
    public class LuisTraceInfo
    {
        [JsonProperty("recognizerResult")]
        public RecognizerResult RecognizerResult { set; get; }

        [JsonProperty("luisResult")]
        public LuisResult Result { set; get; }

        [JsonProperty("luisModel")]
        public LuisApplication Application { set; get; }

        [JsonProperty("luisOptions")]
        public ILuisOptions Options { set; get; }
    }
}
