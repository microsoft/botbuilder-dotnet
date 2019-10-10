using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Templates
{
    /// <summary>
    /// Defines an activity Template where the template expression is local aka "inline" 
    /// and processed through registered IActivityGenerator/ILanguageGenerator.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    public class ActivityTemplate : ITemplate<Activity>
    {
        // Fixed text constructor for inline template
        public ActivityTemplate(string template)
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <summary>
        /// Gets or sets the template to evaluate to create the activity.
        /// </summary>
        /// <value>
        /// The template to evaluate to create the activity.
        /// </value>
        public string Template { get; set; }

        public virtual async Task<Activity> BindToData(ITurnContext context, object data)
        {
            if (!string.IsNullOrEmpty(this.Template))
            {
                // if there is a message generator use that
                IActivityGenerator activityGenerator = context.TurnState.Get<IActivityGenerator>();
                if (activityGenerator != null)
                {
                    var result = await activityGenerator.Generate(
                        turnContext: context,
                        template: this.Template,
                        data: data).ConfigureAwait(false);
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
