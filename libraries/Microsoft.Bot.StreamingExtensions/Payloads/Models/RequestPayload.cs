using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal class RequestPayload
    {
        /// <summary>
        /// Request verb, null on responses
        /// </summary>
        [JsonProperty("verb")]
        public string Verb { get; set; }

        /// <summary>
        /// Request path; null on responses
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// Assoicated stream descriptions
        /// </summary>
        [JsonProperty("streams")]
        public List<StreamDescription> Streams { get; set; }
    }
}
