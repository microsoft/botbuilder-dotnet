using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    [Serializable]
    public class QnARequestContext
    {
        [JsonProperty(PropertyName = "previousQnAId")]
        public int PreviousQnAId { get; set; }

        [JsonProperty(PropertyName = "previousUserQuery")]
        public string PreviousUserQuery { get; set; }
    }
}
