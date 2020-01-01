using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class TextEntity : Entity
    {
        public const string TypeName = "text";

        public TextEntity()
            : base(TypeName)
        {
        }

        public TextEntity(string text)
            : base(TypeName)
        {
            Text = text;
        }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
