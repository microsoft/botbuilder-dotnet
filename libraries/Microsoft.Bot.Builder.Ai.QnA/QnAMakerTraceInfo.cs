// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.QnA
{

    /// <summary>
    /// This class represents all the trace info that we collect from the LUIS Recognizer Middleware
    /// </summary>
    public class QnAMakerTraceInfo
    {
        [JsonProperty("queryResults")]
        public QueryResult[] QueryResults { set; get; }

        [JsonProperty("knowledgeBaseId")]
        public string KnowledgeBaseId { get; set; }

        [JsonProperty("scoreThreshold")]
        public float ScoreThreshold { get; set; }

        [JsonProperty("top")]
        public int Top { get; set; }

        [JsonProperty("strictFilters")]
        public Metadata[] StrictFilters { get; set; }

        [JsonProperty("metadataBoost")]
        public Metadata[] MetadataBoost { get; set; }
    }
}
