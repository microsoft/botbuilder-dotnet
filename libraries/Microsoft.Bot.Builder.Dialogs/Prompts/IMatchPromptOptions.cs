using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Prompts
{
    internal interface IMatchPromptOptions
    {
        /// <summary>
        /// Gets or sets the pattern to match.
        /// </summary>
        /// <value>
        /// This is typically a regex pattern.
        /// </value>
        string Match { get; set; }

        /// <summary>
        /// Gets or sets activity to send when pattern not matched.
        /// </summary>
        /// <value>
        /// The activity to send when the pattern is not matched
        /// </value>
        Activity PatternNotMatchedActivity { get; set; }
    }
}
