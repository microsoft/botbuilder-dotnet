using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Protocol.Payloads
{
    public class ResponsePayload
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
