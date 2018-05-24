namespace Microsoft.Bot.Builder.Prompts.Results
{
    public class ConfirmResult : PromptResult
    {
        /// <summary>
        /// The input bool recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public bool Confirmation
        {
            get { return GetProperty<bool>(nameof(Confirmation)); }
            set { this[nameof(Confirmation)] = value; }
        }

        /// <summary>
        /// The input text recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public string Text
        {
            get { return GetProperty<string>(nameof(Text)); }
            set { this[nameof(Text)] = value; }
        }
    }
}