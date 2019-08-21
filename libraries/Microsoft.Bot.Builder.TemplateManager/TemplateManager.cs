// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    /// TemplateManager manages set of ITemplateRenderer implementations.
    /// </summary>
    /// <remarks>
    /// ITemplateRenderer implements.
    /// </remarks>
    public class TemplateManager
    {
        public TemplateManager()
        {
        }

        /// <summary>
        /// Gets or sets template Renderers.
        /// </summary>
        /// <value>
        /// Template Renderers.
        /// </value>
        public List<ITemplateRenderer> Renderers { get; set; } = new List<ITemplateRenderer>();

        /// <summary>
        /// Gets or sets language fallback policy.
        /// </summary>
        /// <value>
        /// Language fallback policy.
        /// </value>
        public List<string> LanguageFallback { get; set; } = new List<string>();

        public static Activity CreateTemplateActivity(string templateId, object data)
        {
            return new Activity()
            {
                Type = "Template",
                Value = new TemplateOptions()
                {
                    TemplateId = templateId,
                    Data = data,
                },
            };
        }

        /// <summary>
        /// Add a template engine for binding templates.
        /// </summary>
        /// <param name="renderer">Data for binding templates.</param>
        /// <returns>Reurns a template manager.</returns>
        public TemplateManager Register(ITemplateRenderer renderer)
        {
            if (!this.Renderers.Contains(renderer))
            {
                this.Renderers.Add(renderer);
            }

            return this;
        }

        /// <summary>
        /// List registered template engines.
        /// </summary>
        /// <returns>List of rendered templates.</returns>
        public IList<ITemplateRenderer> List()
        {
            return this.Renderers;
        }

        public void SetLanguagePolicy(IEnumerable<string> languageFallback)
        {
            LanguageFallback = new List<string>(languageFallback);
        }

        public IEnumerable<string> GetLanguagePolicy()
        {
            return LanguageFallback;
        }

        /// <summary>
        /// Send a reply with the template.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="templateId">Id of the template.</param>
        /// <param name="data">Data to render the template.</param>
        /// <returns>Task.</returns>
        public async Task ReplyWith(ITurnContext turnContext, string templateId, object data = null)
        {
            BotAssert.ContextNotNull(turnContext);

            // apply template
            Activity boundActivity = await RenderTemplate(turnContext, turnContext.Activity?.AsMessageActivity()?.Locale, templateId, data).ConfigureAwait(false);
            if (boundActivity != null)
            {
                await turnContext.SendActivityAsync(boundActivity);
                return;
            }

            return;
        }

        /// <summary>
        /// Render the template.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="language">Template language.</param>
        /// <param name="templateId">The id of the template.</param>
        /// <param name="data">Data to render the tempplate with.</param>
        /// <returns>Task.</returns>
        public async Task<Activity> RenderTemplate(ITurnContext turnContext, string language, string templateId, object data = null)
        {
            var fallbackLocales = new List<string>(LanguageFallback);

            if (!string.IsNullOrEmpty(language))
            {
                fallbackLocales.Add(language);
            }

            fallbackLocales.Add("default");

            // try each locale until successful
            foreach (var locale in fallbackLocales)
            {
                foreach (var renderer in this.Renderers)
                {
                    object templateOutput = await renderer.RenderTemplate(turnContext, locale, templateId, data);
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
            }

            return null;
        }
    }
}
