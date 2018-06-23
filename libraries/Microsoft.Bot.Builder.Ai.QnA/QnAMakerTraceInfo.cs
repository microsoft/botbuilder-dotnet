// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.QnA
{
    /// <summary>
    /// This class represents all the trace info that we collect from the QnAMaker Middleware
    /// </summary>
    public class QnAMakerTraceInfo
    {
        /// <summary>
        /// Message which instigated the query to QnAMaker
        /// </summary>
        [JsonProperty("message")]
        public IMessageActivity Message { set; get; }

        /// <summary>
        /// Results that QnAMaker returned
        /// </summary>
        [JsonProperty("queryResults")]
        public QueryResult[] QueryResults { set; get; }

        /// <summary>
        /// ID of the Knowledgebase that is being used
        /// </summary>
        [JsonProperty("knowledgeBaseId")]
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Questions with a match of less than the score threshold are not returned
        /// </summary>
        [JsonProperty("scoreThreshold")]
        public float ScoreThreshold { get; set; }

        /// <summary>
        /// Number of ranked results that are asked to be returned
        /// </summary>
        [JsonProperty("top")]
        public int Top { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("strictFilters")]
        public Metadata[] StrictFilters { get; set; }

        /// <summary>
        /// Miscellaneous data
        /// </summary>
        [JsonProperty("metadataBoost")]
        public Metadata[] MetadataBoost { get; set; }
    }
}
