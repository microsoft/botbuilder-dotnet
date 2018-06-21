using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Alexa.Directives
{
    public class HintDirective : IAlexaDirective
    {
        public string Type => "Hint";
        public Hint Hint { get; set; }
    }

    public class Hint
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TextContentType Type { get; set; }
        public string Text { get; set; }
    }
}
