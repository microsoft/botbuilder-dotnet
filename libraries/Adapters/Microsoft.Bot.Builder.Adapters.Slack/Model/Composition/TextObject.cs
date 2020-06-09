using Microsoft.Bot.Builder.Adapters.Slack.Model.Composition;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Text Object https://api.slack.com/reference/block-kit/composition-objects#text
    /// </summary>
    public class TextObject
    {
        public TextObjectType Type { get; set; }

        public string Text { get; set; }

        public bool Emoji { get; set; }

        public bool Verbatim { get; set; }
    }
}
