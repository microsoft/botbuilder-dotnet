using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class ModelViewState
    {
        [JsonProperty(PropertyName = "values")]
        public Dictionary<string, Dictionary<string, ModalBlock>> Values { get; } = new Dictionary<string, Dictionary<string, ModalBlock>>();
    }
}
