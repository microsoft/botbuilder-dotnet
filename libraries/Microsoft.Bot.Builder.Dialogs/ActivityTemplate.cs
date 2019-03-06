using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{

    /// <summary>
    /// Defines an activity Template where the template expression is local aka "inline".
    /// </summary>
    public class ActivityTemplate : ITemplate<IMessageActivity>
    {
        // Fixed text constructor for inline template
        public ActivityTemplate(string template)
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <summary>
        /// Gets or sets the template to evaluate to create the IMessageActivity.
        /// </summary>
        public string Template { get; set; }

        public async Task<IMessageActivity> BindToData(ITurnContext context, object data, Func<string, object, object> binder)
        {
            if (!string.IsNullOrEmpty(this.Template))
            {
                // if there is a message generator use that
                IMessageActivityGenerator messageGenerator = context.TurnState.Get<IMessageActivityGenerator>();
                if (messageGenerator != null)
                {
                    var result = await messageGenerator.Generate(
                        context.Activity.Locale,
                        inlineTemplate: this.Template,
                        id: null,
                        data: data,
                        tags: null,
                        types: null,
                        binder: binder).ConfigureAwait(false);
                    return result;
                }

                // fallback to just text based LG if there is a language generator
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
                        types: null,
                        valueBinder: binder).ConfigureAwait(false);
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
