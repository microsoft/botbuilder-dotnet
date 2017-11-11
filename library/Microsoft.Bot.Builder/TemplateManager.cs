using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class TemplateManager : IContextCreated, IPostActivity
    {
        private readonly Bot _bot;
        private List<ITemplateEngine >_templateEngines = new List<ITemplateEngine>();

        public TemplateManager()
        {
        }

        /// <summary>
        /// Add a template engine for binding templates
        /// </summary>
        /// <param name="engine"></param>

        public void Register(ITemplateEngine engine)
        {
            if (!this._templateEngines.Contains(engine))
                this._templateEngines.Add(engine);
        }

        /// <summary>
        /// List registered template engines
        /// </summary>
        /// <returns></returns>
        public IList<ITemplateEngine> List()
        {
            return this._templateEngines;
        }

        public Task ContextCreated(BotContext context)
        {
            context.TemplateManager = this;
            return Task.CompletedTask;
        }

        public async Task PostActivity(BotContext context, IList<Activity> activities)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);
            // Ensure template activities are bound .
            foreach (var activity in activities)
            {
                if (activity.Type == "template")
                {
                    await this.bindActivityTemplate(context, activity);
                }
            }
        }

        private async Task<Activity> findAndApplyTemplate(BotContext context, string language, string templateId, object data)
        {
            foreach (var templateEngine in this._templateEngines)
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
                    Activity boundActivity = await this.findAndApplyTemplate(context, locale, activity.Text, activity.Value).ConfigureAwait(false);
                    if (boundActivity != null)
                    {
                        foreach(var property in typeof(Activity).GetProperties())
                        {
                            property.SetValue(activity, property.GetValue(boundActivity));
                        }
                        return;
                    }
                }
                throw new Exception($"Could not resolve template id:{ activity.Text}");
            }
        }

    }

    public class TempleEngineMiddleware : IContextCreated
    {
        private ITemplateEngine _templateEngine;

        public TempleEngineMiddleware(ITemplateEngine templateEngine)
        {
            _templateEngine = templateEngine;
        }

        public Task ContextCreated(BotContext context)
        {
            context.TemplateManager.Register(_templateEngine);
            return Task.CompletedTask;
        }

    }

    public static class BotTemplateExtensions
    {
        /// <summary>
        /// Add templateEngine
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="templateEngine"></param>
        /// <returns></returns>
        public static Bot UseTemplateEngine(this Bot bot, ITemplateEngine templateEngine)
        {
            return bot.Use(new TempleEngineMiddleware(templateEngine));
        }
    }
}