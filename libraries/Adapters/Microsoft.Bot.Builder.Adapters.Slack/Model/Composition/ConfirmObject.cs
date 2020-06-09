namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Confirmation Dialog Object https://api.slack.com/reference/block-kit/composition-objects#confirm
    /// </summary>
    public class ConfirmObject
    {
        public TextObject Title { get; set; }

        public TextObject Text { get; set; }

        public TextObject Confirm { get; set; }

        public TextObject Deny { get; set; }

        public string Style { get; set; }
    }
}
