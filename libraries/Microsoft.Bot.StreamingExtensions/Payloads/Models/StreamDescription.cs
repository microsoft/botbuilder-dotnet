using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    public class StreamDescription
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("length")]
        public int? Length { get; set; }
    }
}
