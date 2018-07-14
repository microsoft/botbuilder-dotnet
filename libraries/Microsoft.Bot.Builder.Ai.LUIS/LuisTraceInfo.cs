// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Luis
{
    /// <summary>
    /// This class represents all the trace info that we collect from the LUIS Recognizer Middleware.
    /// </summary>
    public class LuisTraceInfo
    {
        /// <summary>
        /// Gets or sets raw recognizer result.
        /// </summary>
        [JsonProperty("recognizerResult")]
        public RecognizerResult RecognizerResult { get; set; }

        /// <summary>
        /// Gets or sets processed result.
        /// </summary>
        [JsonProperty("luisResult")]
        public LuisResult LuisResult { get; set; }

        /// <summary>
        /// Gets or sets the application being called.
        /// </summary>
        [JsonProperty("LuisApplication")]
        public LuisApplication Application { get; set; }

        /// <summary>
        /// Gets or sets prediction options.
        /// </summary>
        [JsonProperty("luisOptions")]
        public LuisPredictionOptions Options { get; set; }
    }
}
