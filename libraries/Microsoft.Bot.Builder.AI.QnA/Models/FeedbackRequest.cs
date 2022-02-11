// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Active learning feedback records request. </summary>
    public class FeedbackRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackRequest"/> class.
        /// </summary>
        /// <param name="records">FeedbackRecords.</param>
        public FeedbackRequest(List<FeedbackRecord> records)
        {
            Records = records;
        }

        /// <summary>
        /// Gets FeedbackRecords.
        /// </summary>
        /// <value>FeedbackRecords.</value>
        [JsonProperty("records")]
        public List<FeedbackRecord> Records { get; }
    }
}
