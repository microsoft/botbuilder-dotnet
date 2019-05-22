using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    [Serializable]
    public class QnAPrompts
    {
        [JsonProperty("displayOrder")]
        public int DisplayOrder { get; set; }

        [JsonProperty("qna")]
        public int? Qna { get; set; }

        [JsonProperty("qnaId")]
        public int QnaId { get; set; }

        [JsonProperty("displayText")]
        public string DisplayText { get; set; }
    }
}
