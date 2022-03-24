﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.AI.QnA.Models;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Defines options for the QnA Maker knowledge base.
    /// </summary>
    public class QnAMakerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerOptions"/> class.
        /// </summary>
        public QnAMakerOptions()
        {
            ScoreThreshold = 0.3f;
        }

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
        /// Gets or sets the time in milliseconds to wait before the request times out.
        /// </summary>
        /// <value>
        /// The time in milliseconds to wait before the request times out. Default is 100000 milliseconds.
        /// </value>
        /// <remarks>
        /// This property allows users to set Timeout without having to pass in a custom HttpClient to QnAMaker class constructor.
        /// If using custom HttpClient, then set Timeout value in HttpClient instead of QnAMakerOptions.Timeout.
        /// </remarks>
        [JsonProperty("timeout")]
        public double Timeout { get; set; }

        /// <summary>
        /// Gets or sets the number of ranked results you want in the output.
        /// </summary>
        /// <value>
        /// The number of ranked results you want in the output.
        /// </value>
        [JsonProperty("top")]
        public int Top { get; set; }

        /// <summary>
        /// Gets or sets context of the previous turn.
        /// </summary>
        /// <value>
        /// The context of previous turn.
        /// </value>
        [JsonProperty("context")]
        public QnARequestContext Context { get; set; }

        /// <summary>
        /// Gets or sets QnA Id of the current question asked (if available).
        /// </summary>
        /// <value>
        /// Id of the current question asked.
        /// </value>
        [JsonProperty("qnAId")]
        public int QnAId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Metadata"/> collection to be sent when calling QnA Maker to filter results.
        /// </summary>
        /// <value>
        /// An array of <see cref="Metadata"/>.
        /// </value>
        [JsonProperty("strictFilters")]
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public Metadata[] StrictFilters { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets the <see cref="Metadata"/> collection to be sent when calling QnA Maker to boost results.
        /// </summary>
        /// <value>
        /// An array of <see cref="Metadata"/>.
        /// </value>
        [Obsolete("This property is no longer used and will be ignored")]
        [JsonIgnore]
#pragma warning disable CA1819 // Properties should not return arrays (property is obsolete, we won't change it)
        public Metadata[] MetadataBoost { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets a value indicating whether to call test or prod environment of knowledge base to be called. 
        /// </summary>
        /// <value>
        /// A value indicating whether to call test or prod environment of knowledge base. 
        /// </value>
        [JsonProperty("isTest")]
        public bool IsTest { get; set; }

        /// <summary>
        /// Gets or sets the QnA Maker ranker type to use.
        /// </summary>
        /// <value>
        /// The QnA Maker ranker type to use.
        /// </value>
        /// <seealso cref="RankerTypes"/>
        [JsonProperty("rankerType")]
        public string RankerType { get; set; }

        /// <summary>
        /// Gets or sets logical operation (OR/AND) for <see cref="StrictFilters"/>.
        /// </summary>
        /// <value>Logical operation (OR/AND) for <see cref="StrictFilters"/>.</value>
        [JsonProperty("strictFiltersJoinOperator")]
        public JoinOperator StrictFiltersJoinOperator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include precise answer in response.
        /// </summary>
        /// <value>
        /// True/False, defaults to true.
        /// </value>
        [JsonProperty("enablePreciseAnswer")]
        public BoolExpression EnablePreciseAnswer { get; set; } = true;

        /// <summary>
        /// Gets or sets the metadata and sources used to filter QnA Maker results.
        /// </summary>
        /// <value>
        /// An object with metadata, source filters and corresponding operators.
        /// </value>
        [JsonProperty("filters")]
        public Filters Filters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include Unstructured Sources from the Knowledge Base to query.
        /// </summary>
        /// <value> Flag to enable Query over Unstructured Sources. True/False, defaults to True.</value>
        [JsonProperty("includeUnstructuredSources")]
        public BoolExpression IncludeUnstructuredSources { get; set; } = true;
    }
}
