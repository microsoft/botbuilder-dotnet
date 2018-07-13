using System;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// An activity by which a bot can log internal information into a logged conversation transcript.
    /// </summary>
    public class TraceActivity : ActivityWithValue
    {
        public TraceActivity() : base(ActivityTypes.Trace)
        {
        }

        /// <summary>
        /// Gets or sets the name of the trace
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets reference to another conversation or activity
        /// </summary>
        [JsonProperty(PropertyName = "relatesTo")]
        public ConversationReference RelatesTo { get; set; }
    }
}
