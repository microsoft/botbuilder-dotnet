using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines an activity Template where the template expression is local aka "inline".
    /// </summary>
    [DebuggerDisplay("{Template}")]
    public class ActivityTemplate : IActivityTemplate
    {
        // Fixed text constructor for inline template
        public ActivityTemplate(string template)
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <summary>
        /// Gets or sets the template to evaluate to create the IMessageActivity.
        /// </summary>
        /// <value>
        /// The template to evaluate to create the IMessageActivity.
        /// </value>
        public string Template { get; set; }

        public async Task<Activity> BindToData(ITurnContext context, object data)
        {
            if (!string.IsNullOrEmpty(this.Template))
            {
                // if there is a message generator use that
                IMessageActivityGenerator messageGenerator = context.TurnState.Get<IMessageActivityGenerator>();
                if (messageGenerator != null)
                {
                    var result = await messageGenerator.Generate(
                        turnContext: context,
                        template: this.Template,
                        data: data).ConfigureAwait(false);
                    return result as Activity;
                }

                // fallback to just text based LG if there is a language generator
                var message = Activity.CreateMessageActivity();
                message.Text = this.Template;
                message.Speak = this.Template;

                ILanguageGenerator languageGenerator = context.TurnState.Get<ILanguageGenerator>();
                if (languageGenerator != null)
                {
                    var result = await languageGenerator.Generate(
                        turnContext: context,
                        template: Template,
                        data: data).ConfigureAwait(false);
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

        public override string ToString()
        {
            return $"{nameof(ActivityTemplate)}({this.Template})";
        }
    }
}
