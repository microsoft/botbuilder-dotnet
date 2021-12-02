using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Active learning feedback records request. </summary>
    public class FeedbackRequest
    {
        /// <summary>
        /// FeedbackRecords
        /// </summary>
        [JsonProperty("records")]
        public List<FeedbackRecord> Records { get; set; }
    }
}
