using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{

    /// <summary>
    /// Template for simple text responses
    /// </summary>
    public class TextActivity : IActivityTemplate
    {

        // Fixed text constructor for inline template
        public TextActivity(string template)
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        public string Template { get; set; }

        public async Task<Activity> BindToActivity(ITurnContext context, object data)
        {
            if (!string.IsNullOrEmpty(this.Template))
            {
                var message = Activity.CreateMessageActivity();
                message.Text = this.Template;
                message.Speak = this.Template;

                ILanguageGenerator languageGenerator = context.TurnState.Get<ILanguageGenerator>();
                if (languageGenerator != null)
                {
                    var result = await languageGenerator.Generate(
                        context.Activity.Locale,
                        inlineTemplate: Template,
                        id: null,
                        data: data,
                        tags: null,
                        types: null).ConfigureAwait(false);
                    if (result != null)
                    {
                        message.Text = result;
                        message.Speak = result;
                    }
                }

                return message as Activity;
            }

            return null;
        }
    }
}
