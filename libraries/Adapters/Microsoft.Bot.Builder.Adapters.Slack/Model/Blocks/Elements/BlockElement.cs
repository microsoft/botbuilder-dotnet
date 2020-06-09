using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    public abstract class BlockElement
    {
        [JsonExtensionData(ReadData = true, WriteData = true)]
        private IDictionary<string, JToken> AdditionalProperties { get; set; }
    }
}
