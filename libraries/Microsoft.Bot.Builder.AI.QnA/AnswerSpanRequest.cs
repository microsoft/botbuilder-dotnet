using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    public class  AnswerSpanRequest
    {
        /// <summary>
        /// Gets or sets the enablet.
        /// </summary>
        /// <value>
        /// The answer text.
        /// </value>
        [JsonProperty("enable")]
        public Boolean Enable { get; set; }

        /// <summary>
        /// Gets or sets the TopAnswersWithSpan.
        /// </summary>
        /// <value>
        /// The answer TopAnswersWithSpan.
        /// </value>
        [JsonProperty("topAnswersWithSpan")]

        public int TopAnswersWithSpan { get; set; }
    }
}
