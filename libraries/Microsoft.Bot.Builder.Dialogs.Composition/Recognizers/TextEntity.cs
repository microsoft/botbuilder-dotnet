using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public class TextEntity : Entity
    {
        public const string TypeName = "Text";

        public TextEntity()
            : base(TypeName)
        {
        }

        public TextEntity(string text)
            : base(TypeName)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}
