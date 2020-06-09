using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    public abstract class Block
    {
        [JsonProperty(PropertyName = "block_id")]
        public string BlockId { get; set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]
        private IDictionary<string, JToken> AdditionalProperties { get; set; }
    }
}
