using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class PostToAdapterMiddleware : IPostActivity
    {
        private readonly Bot _bot;

        public PostToAdapterMiddleware(Bot b)
        {
            _bot = b ?? throw new ArgumentNullException(nameof(Bot));
        }

        public async Task PostActivity(BotContext context, IList<Activity> activities)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);
            // Ensure template activities are bound .
            foreach(var activity in activities)
            {
                if (activity.Type == "template")
                {
                    await this.bindActivityTemplate(context, activity);
                }
                if (String.IsNullOrEmpty(activity.Type))
                {
                    activity.Type = ActivityTypes.Message;
                }
            }

            await _bot.Adapter.Post(activities).ConfigureAwait(false);
        }

        private async Task<Activity> findAndApplyTemplate(BotContext context, string language, string templateId, object data)
        {
            var templateEngines = ((BotContext)context).TemplateEngines;

            foreach (var templateEngine in templateEngines)
            {
                object templateOutput = await templateEngine.RenderTemplate(context, language, templateId, data);
                if (templateOutput != null)
                {
                    if (templateOutput is string)
                    {
                        return new Activity(type: ActivityTypes.Message, text: (string)templateOutput);
                    }
                    else
                    {
                        return templateOutput as Activity;
                    }
                }
            }
            return null;
        }


        private async Task bindActivityTemplate(BotContext context, Activity activity)
        {
            //TODO
            List<string> fallbackLocales = new List<string>();
            if (!String.IsNullOrEmpty(context.Request.Locale))
                fallbackLocales.Add(context.Request.Locale);
            fallbackLocales.Add("default");

            // Ensure activities are well formed.
            // bind any template activity
            if (activity.Type == "template")
            {
                // try each locale until successful
                foreach (var locale in fallbackLocales)
                {
                    // apply template
                    var boundActivity = await this.findAndApplyTemplate(context, locale, activity.Text, activity.Value).ConfigureAwait(false);
                    if (boundActivity != null)
                    {
                        // merge on top of existing activity
                        foreach (var property in boundActivity.Properties)
                        {
                            ((dynamic)activity)[property.Key] = property.Value;
                        }
                        return;
                    }
                }
                throw new Exception($"Could not resolve template id:{ activity.Text}");
            }
        }
    }
}