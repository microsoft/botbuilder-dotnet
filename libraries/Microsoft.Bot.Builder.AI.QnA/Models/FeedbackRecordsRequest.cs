// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Active learning feedback records request. </summary>
    public class FeedbackRecordsRequest
    {
        /// <summary>
        /// Gets A list of <see cref="FeedbackRecord"/> for Active Learning.
        /// </summary>
        /// <value>A list of <see cref="FeedbackRecord"/> for Active Learning.</value>
        [JsonProperty("records")]
        public List<FeedbackRecord> Records { get; } = new List<FeedbackRecord>();
    }
}
