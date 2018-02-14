// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Middleware
{
    public class TemplateManager : IContextCreated, ISendActivity
    {
        public const string TEMPLATE = "template";        
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
        
        public async Task ContextCreated(IBotContext context, Middleware.MiddlewareSet.NextDelegate next)
        {
            context.TemplateManager = this;
            await next().ConfigureAwait(false);             
        }

        public async Task SendActivity(IBotContext context, IList<IActivity> activities, Middleware.MiddlewareSet.NextDelegate next)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);
            // Ensure template activities are bound .
            foreach (var activity in activities)
            {
                if (activity.Type == TEMPLATE)
                {
                    await this.BindActivityTemplate(context, activity).ConfigureAwait(false);
                }
            }

            await next().ConfigureAwait(false); 
        }

        private async Task<Activity> FindAndApplyTemplate(IBotContext context, string language, string templateId, object data)
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

        private async Task BindActivityTemplate(IBotContext context, IActivity activity)
        {
            List<string> fallbackLocales = new List<string>(this._languageFallback);

            string requestLocale = context.Request?.AsMessageActivity()?.Locale;
            if (!String.IsNullOrEmpty(requestLocale))
            {
                fallbackLocales.Add(requestLocale);
            }

            fallbackLocales.Add("default");

            // Ensure activities are well formed.
            // bind any template activity
            if (activity.Type == TemplateManager.TEMPLATE)
            {
                string messageText = ((Activity)activity).Text; 
                object messageValue = ((Activity)activity).Value;

                // try each locale until successful
                foreach (var locale in fallbackLocales)
                {
                    // apply template
                    Activity boundActivity = 
                        await this.FindAndApplyTemplate(context, locale, messageText, messageValue).ConfigureAwait(false);

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
                throw new Exception($"Could not resolve template id:{ ((Activity)activity).Text}");
            }
        }       
    }

    public class TemplateRendererMiddleware : Middleware.IContextCreated
    {
        private ITemplateRenderer _templateEngine;

        public TemplateRendererMiddleware(ITemplateRenderer templateEngine)
        {
            _templateEngine = templateEngine;
        }
        
        public async Task ContextCreated(IBotContext context, Middleware.MiddlewareSet.NextDelegate next)
        {
            context.TemplateManager.Register(_templateEngine);
            await next().ConfigureAwait(false); 
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