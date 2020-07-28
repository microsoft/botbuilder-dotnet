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
#pragma warning disable CA1724 // Type names should not match namespaces (by design and we can't change this without breaking binary compat)
    public class TemplateManager
#pragma warning restore CA1724 // Type names should not match namespaces
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateManager"/> class.
        /// </summary>
        public TemplateManager()
        {
        }

        /// <summary>
        /// Gets or sets template renderers.
        /// </summary>
        /// <value>
        /// Template renderers.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat)
        public List<ITemplateRenderer> Renderers { get; set; } = new List<ITemplateRenderer>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets language fall-back policy.
        /// </summary>
        /// <value>
        /// Language fall-back policy.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat)
        public List<string> LanguageFallback { get; set; } = new List<string>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Creates an Activity object of type equal to "Template" and value equal to a new TemplateOptions object.
        /// </summary>
        /// <param name="templateId">The template Id to use in the new TemplateOptions object.</param>
        /// <param name="data">The data to use in the new TemplateOptions object.</param>
        /// <returns>An Activity object.</returns>
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
        /// <returns>Returns a template manager.</returns>
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

        /// <summary>
        /// Initializes the LanguageFallback property of the current instance.
        /// </summary>
        /// <param name="languageFallback">A list of strings that define the LanguageFallback property.</param>
        public void SetLanguagePolicy(IEnumerable<string> languageFallback)
        {
            LanguageFallback = new List<string>(languageFallback);
        }

        /// <summary>
        /// Gets the value of the LanguageFallback property of the current instance.
        /// </summary>
        /// <returns>A list of strings equal to the LanguageFallback property.</returns>
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
#pragma warning disable UseAsyncSuffix // Use Async suffix (we can't change this without breaking compat)
        public async Task ReplyWith(ITurnContext turnContext, string templateId, object data = null)
#pragma warning restore UseAsyncSuffix // Use Async suffix
        {
            BotAssert.ContextNotNull(turnContext);

            // apply template
            Activity boundActivity = await RenderTemplate(turnContext, turnContext.Activity?.AsMessageActivity()?.Locale, templateId, data).ConfigureAwait(false);
            if (boundActivity != null)
            {
                await turnContext.SendActivityAsync(boundActivity).ConfigureAwait(false);
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
        /// <param name="data">Data to render the template with.</param>
        /// <returns>Task.</returns>
#pragma warning disable UseAsyncSuffix // Use Async suffix (we can't change this without breaking compat)
        public async Task<Activity> RenderTemplate(ITurnContext turnContext, string language, string templateId, object data = null)
#pragma warning restore UseAsyncSuffix // Use Async suffix
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
                    object templateOutput = await renderer.RenderTemplate(turnContext, locale, templateId, data).ConfigureAwait(false);
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
