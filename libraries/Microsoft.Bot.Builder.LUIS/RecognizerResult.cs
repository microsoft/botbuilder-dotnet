using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.LUIS
{
    public class RecognizerResult
    {
        public string Text { set; get; }
        public string AlteredText { set; get; }
        public JObject Intents { get; set; }
        public JObject Entities { get; set; }
    }
}
