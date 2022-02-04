// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Active learning feedback records.
    /// </summary>
    public class FeedbackRecords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackRecords"/> class.
        /// </summary>
        /// <param name="records">The list of feedback records.</param>
        public FeedbackRecords(Collection<FeedbackRecord> records)
        {
            Records = records;
        }

        /// <summary>
        /// Gets the list of feedback records.
        /// </summary>
        /// <value>
        /// List of feedback records.
        /// </value>
        [JsonProperty("feedbackRecords")]
        public Collection<FeedbackRecord> Records { get; }
    }
}
