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
        public const string TEMPLATE = "template";

        private readonly Bot _bot;
        private List<ITemplateRenderer> _templateRenderers = new List<ITemplateRenderer>();
        private List<string> _languageFallback = new List<string>();

        public TemplateManager()
        {
        }

        /// <summary>
        /// Add a template engine for binding templates
        /// </summary>
        /// <param name="engine"></param>

        public void Register(ITemplateRenderer engine)
        {
            if (!this._templateRenderers.Contains(engine))
                this._templateRenderers.Add(engine);
        }

        /// <summary>
        /// List registered template engines
        /// </summary>
        /// <returns></returns>
        public IList<ITemplateRenderer> List()
        {
            return this._templateRenderers;
        }

        public void SetLanguagePolicy(IEnumerable<string> languageFallback)
        {
            this._languageFallback = new List<string>(languageFallback);
        }

        public IEnumerable<string> GetLanguagePolicy()
        {
            return this._languageFallback;
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
                if (activity.Type == TEMPLATE)
                {
                    await this.bindActivityTemplate(context, activity);
                }
            }
        }

        private async Task<Activity> findAndApplyTemplate(BotContext context, string language, string templateId, object data)
        {
            foreach (var renderer in this._templateRenderers)
            {
                object templateOutput = await renderer.RenderTemplate(context, language, templateId, data);
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
            List<string> fallbackLocales = new List<string>(this._languageFallback);
            if (!String.IsNullOrEmpty(context.Request.Locale))
                fallbackLocales.Add(context.Request.Locale);
            fallbackLocales.Add("default");

            // Ensure activities are well formed.
            // bind any template activity
            if (activity.Type == TemplateManager.TEMPLATE)
            {
                // try each locale until successful
                foreach (var locale in fallbackLocales)
                {
                    // apply template
                    Activity boundActivity = await this.findAndApplyTemplate(context, locale, activity.Text, activity.Value).ConfigureAwait(false);
                    if (boundActivity != null)
                    {
                        lock (activity)
                        {
                            foreach (var property in typeof(Activity).GetProperties())
                            {
                                var value = property.GetValue(boundActivity);
                                if (value != null)
                                    property.SetValue(activity, value);
                            }
                            return;
                        }
                    }
                }
                throw new Exception($"Could not resolve template id:{ activity.Text}");
            }
        }

    }

    public class TemplateRendererMiddleware : IContextCreated
    {
        private ITemplateRenderer _templateEngine;

        public TemplateRendererMiddleware(ITemplateRenderer templateEngine)
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
        /// Add template renderer
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="renderer"></param>
        /// <returns></returns>
        public static Bot UseTemplateRenderer(this Bot bot, ITemplateRenderer renderer)
        {
            return bot.Use(new TemplateRendererMiddleware(renderer));
        }
    }
}