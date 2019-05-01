using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NJsonSchema;

namespace Microsoft.BotBuilderSamples.Tests.Utils.Luis
{
    public class BatchTestItem
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("intent")]
        public string Intent { get; set; }

        [JsonProperty("entities")]
        public BatchTestEntity[] BatchTestEntities { get; set; }
    }
}
