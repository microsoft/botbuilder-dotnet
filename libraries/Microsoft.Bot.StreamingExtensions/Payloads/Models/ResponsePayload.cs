using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal class ResponsePayload
    {
        /// <summary>
        /// Status - The Response Status
        /// </summary>
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Assoicated stream descriptions
        /// </summary>
        [JsonProperty("streams")]
        public List<StreamDescription> Streams { get; set; }
    }
}
