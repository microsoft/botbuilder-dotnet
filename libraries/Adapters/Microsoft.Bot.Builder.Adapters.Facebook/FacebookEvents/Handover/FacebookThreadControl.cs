using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover
{
    public class FacebookThreadControl
    {
        /// <summary>
        /// Gets or Sets the message sent from the requester.
        /// </summary>
        /// <remarks>
        /// Example: "All yours!".
        /// </remarks>
        /// <value>
        /// Message sent from the requester.
        /// </value>
        [JsonProperty("metadata")]
        public string Metadata { get; set; }
    }
}
