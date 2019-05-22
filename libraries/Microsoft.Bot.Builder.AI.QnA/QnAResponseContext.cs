using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    [Serializable]
    public class QnAResponseContext
    {
        [JsonProperty(PropertyName = "prompts")]
        public QnAPrompts[] Prompts { get; set; }
    }
}
