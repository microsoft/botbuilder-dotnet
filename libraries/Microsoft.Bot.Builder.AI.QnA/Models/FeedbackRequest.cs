using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Active learning feedback records request. </summary>
    public class FeedbackRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackRequest"/> class.
        /// </summary>
        /// <param name="records">feedback records.</param>
        public FeedbackRequest(List<FeedbackRecord> records)
        {
            Records = records;
        }

        /// <summary>
        /// Gets the  list of feedbackRecords.
        /// </summary>
        /// <value>
        /// A value with a list of <see cref="FeedbackRecord"/> objects.
        /// </value>
        [JsonProperty("records")]
        public List<FeedbackRecord> Records { get; }
    }
}
