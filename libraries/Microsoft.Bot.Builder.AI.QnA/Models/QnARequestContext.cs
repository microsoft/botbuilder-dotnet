// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// The context associated with QnA.  Used to mark if the current prompt is relevant with a previous question or not.
    /// </summary>
    public class QnARequestContext
    {
        /// <summary>
        /// Gets or sets the previous QnA Id that was returned.
        /// </summary>
        /// <value>
        /// The previous QnA Id.
        /// </value>
        [JsonProperty(PropertyName = "previousQnAId")]
        public int PreviousQnAId { get; set; }

        /// <summary>
        /// Gets or sets the previous user query/question.
        /// </summary>
        /// <value>
        /// The previous user query.
        /// </value>
        [JsonProperty(PropertyName = "previousUserQuery")]
        public string PreviousUserQuery { get; set; } = string.Empty;
    }
}
