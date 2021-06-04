// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// This class represents all the trace info that we collect from the QnAMaker Middleware.
    /// </summary>
    public class QnAMakerTraceInfo
    {
        /// <summary>
        /// Gets or sets message which instigated the query to QnAMaker.
        /// </summary>
        /// <value>
        /// Message which instigated the query to QnAMaker.
        /// </value>
        [JsonProperty("message")]
        public Activity Message { get; set; }

        /// <summary>
        /// Gets or sets results that QnAMaker returned.
        /// </summary>
        /// <value>
        /// Results that QnAMaker returned.
        /// </value>
        [JsonProperty("queryResults")]
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public QueryResult[] QueryResults { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets iD of the Knowledgebase that is being used.
        /// </summary>
        /// <value>
        /// ID of the Knowledgebase that is being used.
        /// </value>
        [JsonProperty("knowledgeBaseId")]
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets the minimum score threshold, used to filter returned results.
        /// </summary>
        /// <remarks>Scores are normalized to the range of 0.0 to 1.0
        /// before filtering.</remarks>
        /// <value>
        /// The minimum score threshold, used to filter returned results.
        /// </value>
        [JsonProperty("scoreThreshold")]
        public float ScoreThreshold { get; set; }

        /// <summary>
        /// Gets or sets number of ranked results that are asked to be returned.
        /// </summary>
        /// <value>
        /// Number of ranked results that are asked to be returned.
        /// </value>
        [JsonProperty("top")]
        public int Top { get; set; }

        /// <summary>
        /// Gets or sets the filters used to return answers that have the specified metadata.       
        /// </summary>
        /// <value>
        /// The filters used to return answers that have the specified metadata.
        /// </value>        
        [JsonProperty("strictFilters")]
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public Metadata[] StrictFilters { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets context for multi-turn responses.
        /// </summary>
        /// <value>
        /// The context from which the QnA was extracted.
        /// </value>
        [JsonProperty("context")]
        public QnARequestContext Context { get; set; }

        /// <summary>
        /// Gets or sets QnA Id of the current question asked.
        /// </summary>
        /// <value>
        /// Id of the current question asked.
        /// </value>
        [JsonProperty("qnaId")]
        public int QnAId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets environment of knowledgebase to be called. 
        /// </summary>
        /// <value>
        /// A value indicating whether to call test or prod environment of knowledgebase. 
        /// </value>
        [JsonProperty("isTest")]
        public bool IsTest { get; set; }

        /// <summary>
        /// Gets or sets ranker Types.
        /// </summary>
        /// <value>
        /// Ranker Types.
        /// </value>
        [JsonProperty("rankerType")]
        public string RankerType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Metadata"/> collection to be sent when calling QnA Maker to boost results.
        /// </summary>
        /// <value>
        /// An array of <see cref="Metadata"/>.
        /// </value>
        [Obsolete("This property is no longer used and will be ignored")]
        [JsonIgnore]
#pragma warning disable CA1819 // Properties should not return arrays (this property is obsolete and we won't change it)
        public Metadata[] MetadataBoost { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
