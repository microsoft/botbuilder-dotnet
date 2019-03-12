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
        private List<ITemplateRenderer> _templateRenderers = new List<ITemplateRenderer>();
        private List<string> _languageFallback = new List<string>();

        public TemplateManager()
        {
        }

        /// <summary>
        /// Add a template engine for binding templates.
        /// </summary>
        /// <param name="renderer">Data for binding templates.</param>
        /// <returns>Reurns a template manager.</returns>
        public TemplateManager Register(ITemplateRenderer renderer)
        {
            if (!_templateRenderers.Contains(renderer))
            {
                _templateRenderers.Add(renderer);
            }

            return this;
        }

        /// <summary>
        /// List registered template engines.
        /// </summary>
        /// <returns>List of rendered templates.</returns>
        public IList<ITemplateRenderer> List()
        {
            return _templateRenderers;
        }

        public void SetLanguagePolicy(IEnumerable<string> languageFallback)
        {
            _languageFallback = new List<string>(languageFallback);
        }

        public IEnumerable<string> GetLanguagePolicy()
        {
            return _languageFallback;
        }

        /// <summary>
        /// Send a reply with the template.
        /// </summary>
        /// <param name="turnContext">The context of the turn.</param>
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
        /// <param name="turnContext">Context turn.</param>
        /// <param name="language">Template language.</param>
        /// <param name="templateId">The id of the template.</param>
        /// <param name="data">Data to render the tempplate with.</param>
        /// <returns>Task.</returns>
        public async Task<Activity> RenderTemplate(ITurnContext turnContext, string language, string templateId, object data = null)
        {
            var fallbackLocales = new List<string>(_languageFallback);

            if (!string.IsNullOrEmpty(language))
            {
                fallbackLocales.Add(language);
            }

            fallbackLocales.Add("default");

            // try each locale until successful
            foreach (var locale in fallbackLocales)
            {
                foreach (var renderer in _templateRenderers)
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
