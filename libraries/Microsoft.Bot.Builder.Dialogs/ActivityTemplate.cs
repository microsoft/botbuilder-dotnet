using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{

    public class ActivityTemplate : IActivityTemplate
    {
        private string[] types;

        // Fixed text constructor for inline template
        public ActivityTemplate(string inlineTemplate)
        {
            this.Template = inlineTemplate ?? throw new ArgumentNullException(nameof(inlineTemplate));
        }

        // Fixed text constructor
        public ActivityTemplate(Type type, string property)
        {
            this.types = GetTypeHierarchy(type);
            this.Property = property ?? throw new ArgumentNullException(nameof(Property));
        }

        public ActivityTemplate(Activity activity)
        {
            this.Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        }

        public string Property { get; set; }

        public string Template { get; set; }

        protected Activity Activity { get; set; }

        public async Task<Activity> BindToActivity(ITurnContext context, object data)
        {
            if (Activity != null)
            {
                // TODO walk text and look for bindings?
                return Activity;
            }

            IMessageGenerator messageGenerator = context.TurnState.Get<IMessageGenerator>();
            if (messageGenerator != null)
            {
                var result = await messageGenerator.Generate(
                    context.Activity.Locale,
                    inlineTemplate: Template,
                    id: this.Property,
                    data: data,
                    tags: null,
                    types: types).ConfigureAwait(false);
                return (Activity)result;
            }

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
                        id: this.Property,
                        data: data,
                        tags: null,
                        types: types).ConfigureAwait(false);
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

        public static string[] GetTypeHierarchy(Type type)
        {
            List<string> types = new List<string>();
            while (type != typeof(object))
            {
                types.Add($"{type.Namespace}.{type.Name}");
                type = type.BaseType;
            }

            return types.ToArray();
        }
    }
}
