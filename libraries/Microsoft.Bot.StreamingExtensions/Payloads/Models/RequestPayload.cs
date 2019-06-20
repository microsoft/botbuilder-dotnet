using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Protocol.Payloads
{
    public class RequestPayload
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
