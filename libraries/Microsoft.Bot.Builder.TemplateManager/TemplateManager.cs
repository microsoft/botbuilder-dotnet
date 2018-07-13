using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    /// TemplateManager manages set of ITemplateRenderer implementations
    /// </summary>
    /// <remarks>
    /// ITemplateRenderer implements 
    /// </remarks>
    public class TemplateManager
    {
        private List<ITemplateRenderer> _templateRenderers = new List<ITemplateRenderer>();
        private List<string> _languageFallback = new List<string>();

        public TemplateManager()
        {
        }

        /// <summary>
        /// Add a template engine for binding templates
        /// </summary>
        /// <param name="renderer"></param>

        public TemplateManager Register(ITemplateRenderer renderer)
        {
            if (!this._templateRenderers.Contains(renderer))
                this._templateRenderers.Add(renderer);
            return this;
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

        /// <summary>
        /// Send a reply with the template
        /// </summary>
        /// <param name="context"></param>
        /// <param name="templateId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task ReplyWith(ITurnContext context, string templateId, object data = null)
        {
            BotAssert.ContextNotNull(context);

            var locale = (context.Activity as MessageActivity)?.Locale;

            // apply template
            Activity boundActivity = await this.RenderTemplate(context, locale, templateId, data).ConfigureAwait(false);

            if (boundActivity != null)
            {
                await context.SendActivityAsync(boundActivity);
                return;
            }
            return;
        }


        /// <summary>
        /// Render the template
        /// </summary>
        /// <param name="context"></param>
        /// <param name="language"></param>
        /// <param name="templateId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Activity> RenderTemplate(ITurnContext context, string language, string templateId, object data = null)
        {
            List<string> fallbackLocales = new List<string>(this._languageFallback);

            if (!string.IsNullOrEmpty(language))
            {
                fallbackLocales.Add(language);
            }

            fallbackLocales.Add("default");

            // try each locale until successful
            foreach (var locale in fallbackLocales)
            {
                foreach (var renderer in this._templateRenderers)
                {
                    var templateOutput = await renderer.RenderTemplate(context, locale, templateId, data);

                    if (templateOutput != null)
                    {
                        if (templateOutput is string templateOutputText)
                        {
                            return new MessageActivity
                            {
                                Text = templateOutputText,
                            };
                        }
                        else
                        {
                            return templateOutput as Activity;
                        }
                    }
                }
            }
            return null;
        }
    }
}
