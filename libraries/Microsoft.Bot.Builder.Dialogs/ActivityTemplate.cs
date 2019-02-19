using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface ILanguageGenerator
    {
        string Apply(string template, object data, string locale = null);
    }

    public class ActivityTemplate
    {
        private bool bound = false;
        private bool textOnly = false;
        private Activity activity = null;

        // Fixed text constructor
        public ActivityTemplate(string text, string textFormat = TextFormatTypes.Plain, SuggestedActions suggestedActions = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException(nameof(text));
            }

            this.bound = true;
            this.textOnly = true;
            this.activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Text = text,
                TextFormat = textFormat,
                SuggestedActions = suggestedActions,
            };
        }

        public ActivityTemplate(Attachment attachment, SuggestedActions suggestedActions)
        {
            this.activity = (Activity)MessageFactory.Attachment(attachment);
            this.activity.SuggestedActions = suggestedActions;
        }

        // Data bound constructor
        public ActivityTemplate() { }

        public ILanguageGenerator LanguageGenerator { get; set; }

        public string Template { get; set; }

        // TODO: This will be removed after we move to PromptDialogs, where OnPromptAsync has dialogContext available
        // In this case, ActivityTemplates need to be databound before hand in OnBeforePromptAsync.
        public Activity Activity
        {
            get
            {
                if (!bound)
                {
                    throw new InvalidOperationException("Bind() must be called prior to retrieving the Activity");
                }

                return activity;
            }
        }

        public Activity Bind(object data)
        {
            if (textOnly)
            {
                return Activity;
            }

            bound = true;
            activity = new Activity()
            {
                Type = ActivityTypes.Message,
                Text = LanguageGenerator.Apply(Template, data),
            };

            return activity;
        }
    }
}
