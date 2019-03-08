using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// The TextMessageGenerator implements IMessageActivityGenerator by using ILanguageGenerator
    /// to generate text and then uses simple markdown semantics like chatdown to create complex
    /// attachments such as herocards, image cards, image attachments etc.
    /// </summary>
    public class TextMessageActivityGenerator : IMessageActivityGenerator
    {
        // Fixed text constructor
        public TextMessageActivityGenerator(ILanguageGenerator languageGenerator)
        {
            if (languageGenerator == null)
            {
                throw new ArgumentNullException(nameof(this.LanguageGenerator));
            }

            this.LanguageGenerator = languageGenerator;
        }

        /// <summary>
        /// Gets or sets language generator.
        /// </summary>
        /// <value>
        /// LanguageGenerator to use to get text.
        /// </value>
        public ILanguageGenerator LanguageGenerator { get; set; }

        /// <summary>
        /// Generate the activity 
        /// </summary>
        /// <param name="locale">locale to generate</param>
        /// <param name="inlineTemplate">(optional) inline template definition.</param>
        /// <param name="id">id of the template to generate text.</param>
        /// <param name="data">data to bind the template to.</param>
        /// <param name="types">type hierarchy for type inheritence.</param>
        /// <param name="tags">contextual tags.</param>
        /// <returns>message activity</returns>
        public async Task<IMessageActivity> Generate(string locale, string inlineTemplate, string id, object data, string[] types, string[] tags, Func<string, object, object> binder = null)
        {
            var result = await this.LanguageGenerator.Generate(locale, inlineTemplate, id, data, types, tags, binder).ConfigureAwait(false);

            var activity = Activity.CreateMessageActivity();
            activity.TextFormat = TextFormatTypes.Markdown;

            // if result is multi line
            if (result.IndexOf("\n") > 0)
            {
                // TODO look for [herocard] attachment semantics
                int start = result.IndexOf("[Herocard");
                if (start >= 0)
                {
                    var end = result.IndexOf("]", start++);
                    if (end > 0)
                    {

                        var section = result.Substring(start, end - start).Trim('[', ']');
                        var lines = section.Split('\n');
                        var card = new HeroCard();
                        foreach (var line in lines)
                        {
                            var parts = line.Split('=');
                            switch (parts[0].ToLower())
                            {
                                case "title":
                                    card.Title = line.Substring(parts[0].Length + 1);
                                    break;
                                case "subtitle":
                                    card.Subtitle = line.Substring(parts[0].Length + 1);
                                    break;
                                case "text":
                                    card.Text = line.Substring(parts[0].Length + 1);
                                    break;
                                //case "buttons":
                                //    card.Buttons = line.Substring(parts[0].Length + 1).Split('|').Select(label => new CardAction(;
                                default:
                                    break;
                            }
                        }

                        activity.Attachments.Add(card.ToAttachment());
                        return activity;
                    }
                }

                // look to see if it has complex attachment semantics
                activity.Text = result;
                activity.Speak = result;
                return activity;
            }
            else
            {
                var i = result.IndexOf("||");
                if (i > 0)
                {
                    activity.Text = result.Substring(0, i);
                    activity.Speak = result.Substring(i + 2);
                    return activity;
                }
            }

            // return it simply as text
            activity.Text = result;
            activity.Speak = result;
            return activity;
        }
    }
}
